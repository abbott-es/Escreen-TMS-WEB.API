using WEB.SERVICES.IService;
using WEB.UTILITY.Logger;

namespace WEB.SERVICES.Service
{
    public abstract class BaseService<T> 
    {
        protected readonly IAppLogger<T> _logger;
        protected BaseService(IAppLogger<T> logger)
        {
            _logger = logger;
        }

        protected async Task<TResult> ExecuteWithLoggingAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            string operationName,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation($"Starting operation: {operationName}");
                var result = await operation(ct);
                _logger.LogInformation($"Completed operation: {operationName}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during operation: {operationName}");
                throw;
            }
        }
    }
}
