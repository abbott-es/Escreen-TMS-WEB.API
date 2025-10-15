using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Extension;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ClientController : GenericController<ClientDto>
    {
        public readonly IClientService _clientService;
        public ClientController(IGenericService<ClientDto> genericService, IClientService clientService) : base(genericService)
        {
            _clientService = clientService;
        }

        /// <summary>
        /// Deletes a list of clients by their IDs.
        /// </summary>
        /// <param name="ids">The list of GUIDs representing clients to delete.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response indicating how many clients were deleted or failure.</returns>
        [HttpDelete]
        public override async Task<IActionResult> DeleteListAsync([FromBody] IEnumerable<Guid> ids, CancellationToken ct)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return ApiResponse<string>
                        .Fail(["No client IDs provided"])
                        .ToBadRequestResult();
                }

                var result = await _clientService.DeleteListAsync(ids, ct);
                if (result)
                {
                    return ApiResponse<object>
                        .Ok(new { DeletedCount = ids.Count() }, "Clients deleted")
                        .ToOkResult();
                }
                return ApiResponse<string>
                        .Fail(["Deletion failed"])
                        .ToBadRequestResult();
            }
            catch
            {
                return ApiResponse<string>
                    .Fail(["Internal Server Error"])
                    .ToInternalServerErrorResult();
            }
        }
    }
}
