using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class HelperController : GenericController<HelperDto>
    {
        public HelperController(IGenericService<HelperDto> genericService) : base(genericService)
        {

        }
    }
}
