using System.Text.Json.Serialization;
using WEB.UTILITY.Enums;

namespace WEB.SERVICES.DTO.Generic
{
    public class GatewayDto : IBaseDto
    {
        public Guid GatewayID { get; set; }
        [JsonIgnore]
        public Guid ID => GatewayID;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Method Method { get; set; }
        public string KeyName { get; set; }
        public string GatewayUrl { get; set; }
    }
}
