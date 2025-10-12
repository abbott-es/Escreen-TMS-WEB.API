using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEB.UTILITY.Helper;

namespace WEB.UTILITY.Extension
{
    public static class ApiResponseExtensions
    {
        public static IActionResult ToOkResult<T>(this ApiResponse<T> response)
        {
            return new OkObjectResult(response);
        }

        public static IActionResult ToCreatedResult<T>(this ApiResponse<T> response, string location = "")
        {
            return new CreatedResult(location, response);
        }

        public static IActionResult ToBadRequestResult<T>(this ApiResponse<T> response)
        {
            return new BadRequestObjectResult(response);
        }

        public static IActionResult ToNotFoundResult<T>(this ApiResponse<T> response)
        {
            return new NotFoundObjectResult(response);
        }

        public static IActionResult ToUnauthorizedResult<T>(this ApiResponse<T> response)
        {
            return new UnauthorizedObjectResult(response);
        }

        public static IActionResult ToForbiddenResult<T>(this ApiResponse<T> response)
        {
            return new ObjectResult(response) { StatusCode = StatusCodes.Status403Forbidden };
        }

        public static IActionResult ToInternalServerErrorResult<T>(this ApiResponse<T> response)
        {
            return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}
