using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;

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
