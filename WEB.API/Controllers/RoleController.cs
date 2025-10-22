using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IGeneric;

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
