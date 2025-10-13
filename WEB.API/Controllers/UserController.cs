using Microsoft.AspNetCore.Mvc;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]

    public class UserController : GenericController<UserDto>
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService) : base(userService)
        {
            _userService = userService;
        }

        //[HttpPost("create-auth")]
        //public async Task<IActionResult> CreateUserHash([FromBody] UserDto authDTO, CancellationToken ct)
        //{
        //    var result = await _userService.CreateUserAsync(authDTO, ct);
        //    return result.Match<IActionResult>(
        //        Left: error => BadRequest(new { Error = error }),
        //        Right: id => CreatedAtAction(
        //            nameof(GetById),   // action name
        //            new { id },        // route values must match {id}
        //            authDTO            // response body
        //        )
        //    );
        //}
    }

}
