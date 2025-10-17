using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WEB.UTILITY.Extension;

namespace WEB.UTILITY.Helper
{
    public static class ResultMatcher
    {
        public static async Task<IActionResult> MatchResultAsync<TLeft, TRight>(
            Task<Either<ApiResponse<TLeft>, ApiResponse<TRight>>> resultTask)
        {
            var result = await resultTask;

            return result.Match(
                Left: error => MapToActionResult(error),
                Right: success => MapToActionResult(success)
            );
        }

        private static IActionResult MapToActionResult<T>(ApiResponse<T> response)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.OK => response.ToOkResult(),
                HttpStatusCode.NoContent => response.ToOkNoContent(),
                HttpStatusCode.Created => response.ToCreatedResult(),
                HttpStatusCode.BadRequest => response.ToBadRequestResult(),
                HttpStatusCode.NotFound => response.ToNotFoundResult(),
                HttpStatusCode.Unauthorized => response.ToUnauthorizedResult(),
                HttpStatusCode.Forbidden => response.ToForbiddenResult(),
                HttpStatusCode.InternalServerError => response.ToInternalServerErrorResult(),
                _ => new ObjectResult(response) { StatusCode = (int)response.StatusCode }
            };
        }
    }
}