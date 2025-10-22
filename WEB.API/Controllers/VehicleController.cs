using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IGeneric;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VehicleController : GenericController<VehicleDto>
    {
        public VehicleController(IGenericService<VehicleDto> genericService) : base(genericService)
        {

        }
    }
}
