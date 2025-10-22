namespace WEB.SERVICES.IService.IGeneric
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
