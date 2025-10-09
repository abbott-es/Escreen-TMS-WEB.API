using System.Collections.Generic;

namespace YarpRateLimitedProxy.Models
{
    public class RateLimitPolicyConfig
    {
        public int PermitLimit { get; set; }
        public string Window { get; set; } = "00:00:10";
        public int QueueLimit { get; set; } = 0;
        public string QueueProcessingOrder { get; set; } = "OldestFirst";
    }

    public class RateLimitingConfig
    {
        public Dictionary<string, RateLimitPolicyConfig> Policies { get; set; } = new();
    }
}
