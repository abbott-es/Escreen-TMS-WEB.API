using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity.Generic
{
    public class Gateway : IEntity
    {
        public Guid GatewayID { get; set; }
        public Guid ID => GatewayID;
        public int Method { get; set; }
        public string KeyName { get; set; }
        public string GatewayUrl { get; set; }
    }
}
