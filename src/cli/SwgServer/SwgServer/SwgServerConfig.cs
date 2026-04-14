namespace SwgServer;

internal sealed class SwgServerConfig
{
    public ServerConfig Server { get; set; } = new();
    public SerilogConfig Serilog { get; set; } = new();
    public AuthConfig Auth { get; set; } = new();

    internal sealed class ServerConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 50051;
    }

    internal sealed class SerilogConfig
    {
        public string MinimumLevel { get; set; } = "Debug";
        public ConsoleSinkConfig Console { get; set; } = new();
        public FileSinkConfig File { get; set; } = new();

        internal sealed class ConsoleSinkConfig
        {
            public bool Enabled { get; set; } = true;
            public string OutputTemplate { get; set; } =
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
        }

        internal sealed class FileSinkConfig
        {
            public bool Enabled { get; set; } = true;
            public string Path { get; set; } = "logs/swgserver-.log";
            public string RollingInterval { get; set; } = "Day";
            public int RetainedFileCountLimit { get; set; } = 30;
            public string OutputTemplate { get; set; } =
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}";
        }
    }

    internal sealed class AuthConfig
    {
        public bool Enabled { get; set; } = true;
        public TokenConfig Token { get; set; } = new();

        internal sealed class TokenConfig
        {
            public string HmacSecret { get; set; } = string.Empty;
            public string SignedToken { get; set; } = string.Empty;
        }
    }
}
