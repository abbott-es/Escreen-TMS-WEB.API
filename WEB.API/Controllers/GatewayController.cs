using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IGeneric;
using WEB.UTILITY.Helper;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class GatewayController : GenericController<GatewayDto>
    {
        private readonly IGatewayService _gatewayService;
        public GatewayController(IGenericService<GatewayDto> gatewayGenericService, IGatewayService gatewayService) : base(gatewayGenericService)
        {
            _gatewayService = gatewayService;
        }

        /// <summary>
        /// Retrieves the Gateway Url by key name.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// This endpoint returns the gateway url.
        /// It requires the user to be authenticated.
        /// </remarks>
        /// <returns>
        /// 200 OK with gateway url.
        /// 404 Not Found if no user role is found.
        /// </returns>
        [HttpGet("GetGatewayUrlByKey")]
        public async Task<IActionResult> GetGatewayUrlByKeyAsync([FromQuery] string keyName, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_gatewayService.GetGatewayUrlByKeyAsync(keyName, ct));
        }
    }
}
