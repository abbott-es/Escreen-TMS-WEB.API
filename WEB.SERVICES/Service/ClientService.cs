using System.Net;
using Dapper;
using LanguageExt;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;
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

        public async Task<Either<ApiResponse<string>, ApiResponse<string>>> DeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            try
            {
                await _unitOfWork.ExecuteAsync(async (conn, tran, ct) =>
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
                return Prelude.Right(ApiResponse<string>.Ok("Deleted Successfully"));
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Error deleting clients");
                return Prelude.Left(ApiResponse<string>.Fail(["Error deleting clients"], HttpStatusCode.InternalServerError));
            }
        }
    }
}
