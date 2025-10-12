using Dapper;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.IService;
using WEB.UTILITY.Logger;

namespace WEB.SERVICES.Service
{
    public class ClientService : BaseService<ClientService>, IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ClientService(IAppLogger<ClientService> appLogger, IUnitOfWork unitOfWork) : base(appLogger)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> DeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            try
            {
                return await _unitOfWork.ExecuteAsync(async (conn, tran, ct) =>
                {
                    var userIds = (await conn.QueryAsync<Guid>(
                        "SELECT UserID FROM [Client] WHERE ClientID IN @ClientIDs",
                        new { ClientIDs = ids }, tran)).ToList();

                    var sql = @"DELETE FROM [Location] WHERE ClientID IN @ClientIDs;
                                DELETE FROM [Client] WHERE ClientID IN @ClientIDs;
                                DELETE FROM [UserInfo] WHERE UserID IN @UserIDs;
                                DELETE FROM [User] WHERE UserID IN @UserIDs;";

                    await conn.ExecuteAsync(sql, new { ClientIDs = ids, UserIDs = userIds }, tran);
                    return true;
                }, ct);
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Error deleting clients");
                throw;
            }
        }
    }
}
