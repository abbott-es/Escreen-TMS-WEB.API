using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IGeneric;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LocationController : GenericController<LocationDto>
    {
        public LocationController(IGenericService<LocationDto> genericService) : base(genericService)
        {

        }
    }
}
