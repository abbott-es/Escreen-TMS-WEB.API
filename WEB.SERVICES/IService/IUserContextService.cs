
namespace WEB.SERVICES.IService
{
    public interface IUserContextService
    {
        public string UserId { get; }
        public string Role { get; }
        public string IpAddress { get; }
        public string DeviceInfo { get; }
        public string AccessToken { get; }
    }
}
