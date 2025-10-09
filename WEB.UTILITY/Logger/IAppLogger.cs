
namespace WEB.UTILITY.Logger
{
    public interface IAppLogger<T>
    {
        void LogDebug(string message, params object[] args);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(Exception ex, string message);
    }
}
