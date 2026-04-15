using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Debugging;
using Swg.CV;
using Swg.Grpc;

namespace SwgServer;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && args[0] is "--generate-token" or "-g")
        {
            try
            {
                GenerateToken();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Token generation failed: {ex.Message}");
                return 1;
            }
        }

        try
        {
            var settings = LoadSettings(args);
            ConfigureSerilog(settings);
            SelfLog.Enable(msg => Log.Debug("Serilog SelfLog: {Message}", msg));

            RegisterGlobalExceptionHandlers();

            // Must be called before any window/UIA/GDI calls: works with app.manifest PerMonitorV2 to ensure Swg.FlaUI and Swg.CV share the same DPI semantics and virtual desktop origin
            SwgScreenEnvironment.Initialize(log: static s => Log.Information(s));

            var authInterceptor = CreateAuthInterceptor(settings);
            if (authInterceptor is null && settings.Auth.Enabled)
                return 1;

            var pidLock = AcquirePidLock();

            var server = CreateServer(settings, authInterceptor);
            server.Start();

            Log.Information("SwgServer started, listening on {Host}:{Port}", settings.Server.Host, settings.Server.Port);

            await WaitForShutdownAsync();

            Log.Information("Shutting down SwgServer...");
            try
            {
                await server.ShutdownAsync();
                Log.Information("SwgServer shut down complete");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during SwgServer shutdown");
            }

            return 0;
        }
        catch (JsonException ex)
        {
            Log.Fatal(ex, "Failed to parse configuration file, check appsettings.json format");
            return 1;
        }
        catch (ArgumentException ex)
        {
            Log.Fatal(ex, "Invalid configuration");
            return 1;
        }
        catch (IOException ex)
        {
            Log.Fatal(ex, "Network or file IO error (port may already be in use)");
            return 1;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "SwgServer startup failed");
            return 1;
        }
        finally
        {
            ReleasePidLock();
            await Log.CloseAndFlushAsync();
        }
    }

    private static SwgServerConfig LoadSettings(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddCommandLine(args)
            .Build();

        return config.Get<SwgServerConfig>() ?? new SwgServerConfig();
    }

    private static void ConfigureSerilog(SwgServerConfig settings)
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(ParseLogLevel(settings.Serilog.MinimumLevel));

        if (settings.Serilog.Console.Enabled)
            loggerConfig.WriteTo.Console(outputTemplate: settings.Serilog.Console.OutputTemplate);

        if (settings.Serilog.File.Enabled)
        {
            var rollingInterval = Enum.Parse<RollingInterval>(settings.Serilog.File.RollingInterval, ignoreCase: true);
            loggerConfig.WriteTo.File(
                path: settings.Serilog.File.Path,
                outputTemplate: settings.Serilog.File.OutputTemplate,
                rollingInterval: rollingInterval,
                retainedFileCountLimit: settings.Serilog.File.RetainedFileCountLimit);
        }

        Log.Logger = loggerConfig.CreateLogger();
    }

    private static Interceptor? CreateAuthInterceptor(SwgServerConfig settings)
    {
        if (!settings.Auth.Enabled)
        {
            Log.Warning("API Token authentication is disabled");
            return null;
        }

        if (string.IsNullOrWhiteSpace(settings.Auth.Token.HmacSecret)
            || string.IsNullOrWhiteSpace(settings.Auth.Token.SignedToken))
        {
            Log.Fatal("auth.enabled is on but hmacSecret or signedToken is not configured, run --generate-token first");
            return null;
        }

        var validator = new TokenValidator(settings.Auth.Token);
        Log.Information("API Token authentication enabled");
        return new AuthInterceptor(validator);
    }

    private static Server CreateServer(SwgServerConfig settings, Interceptor? authInterceptor)
    {
        var server = new Server
        {
            Ports = { new ServerPort(settings.Server.Host, settings.Server.Port, ServerCredentials.Insecure) }
        };

        var deadlineInterceptor = new RpcDeadlineInterceptor(settings.Server.DefaultRpcTimeoutMs);
        Interceptor[] interceptors = authInterceptor is null
            ? new Interceptor[] { deadlineInterceptor }
            : new Interceptor[] { authInterceptor, deadlineInterceptor };

        foreach (var definition in SwgGrpcServiceBinder.GetServiceDefinitions(interceptors))
            server.Services.Add(definition);

        return server;
    }

    private static async Task WaitForShutdownAsync()
    {
        var tcs = new TaskCompletionSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            tcs.TrySetResult();
        };
        await tcs.Task;
    }

    private static void RegisterGlobalExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                Log.Fatal(ex, "Unhandled AppDomain exception");
            else
                Log.Fatal("Unhandled non-CLI exception: {Object}", e.ExceptionObject);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }

    private static FileStream? _pidStream;

    private static FileStream AcquirePidLock()
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "swgserver.pid");

        if (File.Exists(fullPath))
        {
            var existingPid = File.ReadAllText(fullPath).Trim();
            if (int.TryParse(existingPid, out var pid) && IsProcessRunning(pid))
                throw new InvalidOperationException($"SwgServer is already running (PID: {pid}), duplicate startup is not allowed");

            Log.Warning("Stale PID file found (PID: {Pid}, process no longer exists), overwriting", pid);
        }

        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var stream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        _pidStream = stream;

        var pidBytes = Encoding.UTF8.GetBytes(Environment.ProcessId.ToString());
        stream.Write(pidBytes, 0, pidBytes.Length);
        stream.Flush(true);

        Log.Debug("PID file created: {Path} (PID: {Pid})", fullPath, Environment.ProcessId);
        return stream;
    }

    private static void ReleasePidLock()
    {
        if (_pidStream is null)
            return;

        var fullPath = _pidStream.Name;
        try
        {
            _pidStream.Dispose();
        }
        catch
        {
            // Ignore failure to release file handle
        }

        _pidStream = null;

        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            Log.Debug("PID file deleted: {Path}", fullPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to delete PID file: {Path}", fullPath);
        }
    }

    private static bool IsProcessRunning(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    private static Serilog.Events.LogEventLevel ParseLogLevel(string level) =>
        Enum.TryParse<Serilog.Events.LogEventLevel>(level, ignoreCase: true, out var result)
            ? result
            : Serilog.Events.LogEventLevel.Debug;

    private static void GenerateToken()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        var hmacKey = RandomNumberGenerator.GetBytes(32);
        var hmacSecret = Convert.ToBase64String(hmacKey);

        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        using var hmac = new HMACSHA256(hmacKey);
        var signedToken = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(plainToken)));

        var baseDir = AppContext.BaseDirectory;
        // dotnet run sets BaseDirectory to bin/..., walk up to find the directory containing .csproj
        var projectDir = FindProjectDir(baseDir);
        var appSettingsPath = Path.Combine(projectDir, "appsettings.json");
        WriteServerConfig(appSettingsPath, hmacSecret, signedToken);

        var clientConfigPath = Path.Combine(projectDir, "swgclient.json");
        WriteClientConfig(clientConfigPath, plainToken, appSettingsPath);

        Log.Information("Token generated and written to config files:");
        Log.Information("  Server: {Path}", appSettingsPath);
        Log.Information("  Client: {Path}", clientConfigPath);

        Log.CloseAndFlush();
    }

    private static string FindProjectDir(string baseDir)
    {
        var dir = new DirectoryInfo(baseDir);
        while (dir is not null)
        {
            if (dir.GetFiles("*.csproj").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        return baseDir;
    }

    private static void WriteServerConfig(string path, string hmacSecret, string signedToken)
    {
        var json = File.Exists(path) ? File.ReadAllText(path) : "{}";
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();

            foreach (var prop in root.EnumerateObject())
            {
                if (prop.NameEquals("auth"))
                {
                    WriteAuthSection(writer, hmacSecret, signedToken);
                    continue;
                }
                prop.WriteTo(writer);
            }

            if (!root.TryGetProperty("auth", out _))
                WriteAuthSection(writer, hmacSecret, signedToken);

            writer.WriteEndObject();
        }

        File.WriteAllText(path, Encoding.UTF8.GetString(stream.ToArray()));

        static void WriteAuthSection(Utf8JsonWriter w, string secret, string token)
        {
            w.WritePropertyName("auth");
            w.WriteStartObject();
            w.WritePropertyName("enabled");
            w.WriteBooleanValue(true);
            w.WritePropertyName("token");
            w.WriteStartObject();
            w.WritePropertyName("hmacSecret");
            w.WriteStringValue(secret);
            w.WritePropertyName("signedToken");
            w.WriteStringValue(token);
            w.WriteEndObject();
            w.WriteEndObject();
        }
    }

    private static void WriteClientConfig(string path, string plainToken, string appSettingsPath)
    {
        string host = "localhost";
        int port = 50051;

        if (File.Exists(appSettingsPath))
        {
            try
            {
                var json = File.ReadAllText(appSettingsPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("server", out var server))
                {
                    if (server.TryGetProperty("host", out var h))
                        host = h.GetString() ?? host;
                    if (server.TryGetProperty("port", out var p))
                        port = p.GetInt32();
                }
            }
            catch
            {
                // Use defaults on parse failure
            }
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("server");
            writer.WriteStartObject();
            writer.WritePropertyName("host");
            writer.WriteStringValue(host);
            writer.WritePropertyName("port");
            writer.WriteNumberValue(port);
            writer.WriteEndObject();
            writer.WritePropertyName("auth");
            writer.WriteStartObject();
            writer.WritePropertyName("token");
            writer.WriteStringValue(plainToken);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        File.WriteAllText(path, Encoding.UTF8.GetString(stream.ToArray()));
    }
}
