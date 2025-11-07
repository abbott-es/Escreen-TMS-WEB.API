namespace WEB.GATEWAY.Models
{
    public class RedisCfg
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string KeyPrefix { get; set; } = string.Empty;
        public bool UseSsl { get; set; }
        public int SyncTimeout { get; set; }
    }
}
