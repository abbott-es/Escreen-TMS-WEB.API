using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RoleController : GenericController<RoleDto>
    {
        public RoleController(IGenericService<RoleDto> genericService) : base(genericService)
        {

        }
    }
}
