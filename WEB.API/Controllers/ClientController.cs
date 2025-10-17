using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;

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
        /// <param name="genericFields">The list of GUIDs representing clients to delete.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response indicating how many clients were deleted or failure.</returns>
        [HttpDelete]
        public override async Task<IActionResult> DeleteListAsync([FromBody] GenericFiendListDto genericFields, CancellationToken ct)
        {
            return await ResultMatcher.MatchResultAsync(_clientService.DeleteListAsync(genericFields.ID, ct));
        }
    }
}
