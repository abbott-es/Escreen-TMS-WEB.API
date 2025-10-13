using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Extension;

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
            var result = await _genericService.GetByIdAsync(id, ct);
            return result.Match<IActionResult>(
                Left: error => ApiResponse<string>
                    .Fail([error], "Entity not found")
                    .ToNotFoundResult(),
                Right: success => ApiResponse<T>
                    .Ok(success, "Entity retrieved")
                    .ToOkResult()
            );
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
            var result = await _genericService.GetAllAsync(ct, includes);
            return result.Match<IActionResult>(
                Left: error => ApiResponse<string>
                    .Fail([error], "No entities found")
                    .ToNotFoundResult(),
                Right: entities => ApiResponse<IEnumerable<T>>
                    .Ok(entities, "Entities retrieved")
                    .ToOkResult()
            );
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
            var result = await _genericService.AddAsync(entity, ct);
            return result.Match<IActionResult>(
                Left: error => ApiResponse<string>
                    .Fail([error], "Creation failed")
                    .ToBadRequestResult(),
                Right: id => ApiResponse<T>
                    .Ok(entity, "Entity created")
                    .ToCreatedResult()
            );
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
            var result = await _genericService.UpdateAsync(entity, ct);
            return result.Match<IActionResult>(
                Left: error => ApiResponse<string>
                    .Fail([error], "Update failed")
                    .ToNotFoundResult(),
                Right: _ => ApiResponse<string>
                    .Ok("Entity updated")
                    .ToOkResult()
            );
        }

        /// <summary>
        /// Deletes a list of entities by their IDs.
        /// </summary>
        /// <param name="ids">The list of GUIDs representing entities to delete.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response indicating how many entities were deleted or failure.</returns>
        [HttpDelete]
        public virtual async Task<IActionResult> DeleteListAsync([FromBody] IEnumerable<Guid> ids, CancellationToken ct)
        {
            if (ids == null || !ids.Any())
            {
                return ApiResponse<string>
                    .Fail(["No IDs provided"])
                    .ToBadRequestResult();
            }

            var result = await _genericService.DeleteListAsync(ids, ct);
            return result.Match<IActionResult>(
                Left: error => ApiResponse<string>
                    .Fail([error], "Deletion failed")
                    .ToNotFoundResult(),
                Right: _ => ApiResponse<object>
                    .Ok(new { DeletedCount = ids.Count() },"Entities deleted")
                    .ToOkResult()
            );
        }
    }
}
