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
    private static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0] is "--generate-token" or "-g")
        {
            GenerateToken();
            return;
        }

        // 必须在任何窗口/UIA/GDI 调用之前：与 app.manifest 的 PerMonitorV2 共同保证 Swg.FlaUI 与 Swg.CV 同一 DPI 语义与虚拟桌面原点
        SwgScreenEnvironment.Initialize(log: static s => Console.WriteLine(s));

        var settings = LoadSettings(args);
        ConfigureSerilog(settings);
        SelfLog.Enable(msg => Console.Error.WriteLine($"[Serilog SelfLog] {msg}"));

        var authInterceptor = CreateAuthInterceptor(settings);
        var server = CreateServer(settings, authInterceptor);
        server.Start();

        Log.Information("SwgServer 已启动，监听 {Host}:{Port}", settings.Server.Host, settings.Server.Port);

        await WaitForShutdownAsync();

        Log.Information("正在关闭 SwgServer...");
        await server.ShutdownAsync();
        Log.Information("SwgServer 已关闭");

        await Log.CloseAndFlushAsync();
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
            Log.Warning("API Token 认证已禁用");
            return null;
        }

        if (string.IsNullOrWhiteSpace(settings.Auth.Token.HmacSecret)
            || string.IsNullOrWhiteSpace(settings.Auth.Token.SignedToken))
        {
            Log.Fatal("auth.enabled 已开启但 hmacSecret 或 signedToken 未配置，请先运行 --generate-token 生成 Token");
            Environment.Exit(1);
            return null;
        }

        var validator = new TokenValidator(settings.Auth.Token);
        Log.Information("API Token 认证已启用");
        return new AuthInterceptor(validator);
    }

    private static Server CreateServer(SwgServerConfig settings, Interceptor? authInterceptor)
    {
        var server = new Server
        {
            Ports = { new ServerPort(settings.Server.Host, settings.Server.Port, ServerCredentials.Insecure) }
        };

        foreach (var definition in SwgGrpcServiceBinder.GetServiceDefinitions(authInterceptor))
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

    private static Serilog.Events.LogEventLevel ParseLogLevel(string level) =>
        Enum.TryParse<Serilog.Events.LogEventLevel>(level, ignoreCase: true, out var result)
            ? result
            : Serilog.Events.LogEventLevel.Debug;

    private static void GenerateToken()
    {
        var hmacKey = RandomNumberGenerator.GetBytes(32);
        var hmacSecret = Convert.ToBase64String(hmacKey);

        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        using var hmac = new HMACSHA256(hmacKey);
        var signedToken = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(plainToken)));

        var baseDir = AppContext.BaseDirectory;
        // dotnet run 时 BaseDirectory 指向 bin/...，向上查找到包含 .csproj 的目录
        var projectDir = FindProjectDir(baseDir);
        var appSettingsPath = Path.Combine(projectDir, "appsettings.json");
        WriteServerConfig(appSettingsPath, hmacSecret, signedToken);

        var clientConfigPath = Path.Combine(projectDir, "swgclient.json");
        WriteClientConfig(clientConfigPath, plainToken, appSettingsPath);

        Console.WriteLine("Token 已生成并写入配置文件：");
        Console.WriteLine($"  服务端：{appSettingsPath}");
        Console.WriteLine($"  客户端：{clientConfigPath}");
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
                    writer.WritePropertyName("auth");
                    writer.WriteStartObject();
                    writer.WritePropertyName("enabled");
                    writer.WriteBooleanValue(true);
                    writer.WritePropertyName("token");
                    writer.WriteStartObject();
                    writer.WritePropertyName("hmacSecret");
                    writer.WriteStringValue(hmacSecret);
                    writer.WritePropertyName("signedToken");
                    writer.WriteStringValue(signedToken);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                    continue;
                }
                prop.WriteTo(writer);
            }

            if (!root.TryGetProperty("auth", out _))
            {
                writer.WritePropertyName("auth");
                writer.WriteStartObject();
                writer.WritePropertyName("enabled");
                writer.WriteBooleanValue(true);
                writer.WritePropertyName("token");
                writer.WriteStartObject();
                writer.WritePropertyName("hmacSecret");
                writer.WriteStringValue(hmacSecret);
                writer.WritePropertyName("signedToken");
                writer.WriteStringValue(signedToken);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        File.WriteAllText(path, Encoding.UTF8.GetString(stream.ToArray()));
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
                // 解析失败时使用默认值
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
