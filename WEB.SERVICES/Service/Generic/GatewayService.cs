using DocumentFormat.OpenXml.Spreadsheet;
using LanguageExt;
using System.Net;
using WEB.DOMAIN.Entity.Generic;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IGeneric;
using WEB.UTILITY.Helper;

namespace WEB.SERVICES.Service.Generic
{
    public class GatewayService : IGatewayService
    {
        private readonly IRepository<Gateway> _gatewayRepository;
        public GatewayService(IRepository<Gateway> gatewayRepository)
        {
            _gatewayRepository = gatewayRepository;
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<string>>> GetGatewayUrlByKeyAsync(string keyName, CancellationToken ct)
        {
            var gatewayUrls = await _gatewayRepository.GetAllAsync(x => x.KeyName == keyName, ct);

            if (gatewayUrls == null || !gatewayUrls.Any())
                return Prelude.Left(ApiResponse<string>.Fail(["No gateway url was found"], HttpStatusCode.NotFound));

            var url = gatewayUrls.FirstOrDefault();
            return Prelude.Right(ApiResponse<string>.Ok(url.GatewayUrl, HttpStatusCode.OK, "Gateway url successfully retrieved"));
        }
    }
}
