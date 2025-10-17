using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenericController<T> : ControllerBase where T : class
    {
        private readonly IGenericService<T> _genericService;

        public GenericController(IGenericService<T> genericService)
        {
            _genericService = genericService;
        }

        /// <summary>
        /// Retrieves a single entity by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the entity to retrieve.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response containing the entity or a not found result.</returns>
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_genericService.GetByIdAsync(id, ct));
        }

        /// <summary>
        /// Retrieves all entities, optionally including related navigation properties.
        /// </summary>
        /// <param name="includes">An array of navigation property paths to include.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response containing the list of entities or a not found result.</returns>
        [HttpGet]
        public virtual async Task<IActionResult> GetAll([FromQuery] string[] includes, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_genericService.GetAllAsync(ct, includes));
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response indicating success or failure.</returns>
        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] T entity, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_genericService.AddAsync(entity, ct));
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity with updated data.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response indicating success or failure.</returns>
        [HttpPut]
        public virtual async Task<IActionResult> Update([FromBody] T entity, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_genericService.UpdateAsync(entity, ct));
        }

        /// <summary>
        /// Deletes a list of entities by their IDs.
        /// </summary>
        /// <param name="genericFiendList">The list of GUIDs representing entities to delete.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response indicating how many entities were deleted or failure.</returns>
        [HttpDelete]
        public virtual async Task<IActionResult> DeleteListAsync([FromBody] GenericFiendListDto genericFiendList, CancellationToken ct)
        {
            return await ResultMatcher.MatchResultAsync(_genericService.DeleteListAsync(genericFiendList.ID, ct));
        }
    }
}
