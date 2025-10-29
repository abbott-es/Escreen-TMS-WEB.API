using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IGeneric;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class GatewayController : GenericController<GatewayDto>
    {
        public GatewayController(IGenericService<GatewayDto> gatewayGenericService) : base(gatewayGenericService)
        {

        }
    }
}
