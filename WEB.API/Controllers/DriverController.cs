using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DriverController : GenericController<DriverDto>
    {
        public DriverController(IGenericService<DriverDto> genericService) : base(genericService)
        {

        }
    }
}
