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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string[] includes, CancellationToken ct = default)
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] T entity, CancellationToken ct = default)
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

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] T entity, CancellationToken ct = default)
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

        [HttpDelete]
        public async Task<IActionResult> DeleteListAsync([FromBody] IEnumerable<Guid> ids, CancellationToken ct)
        {
            if (ids == null || !ids.Any())
            {
                return ApiResponse<string>
                    .Fail(["No IDs provided"])
                    .ToBadRequestResult();
            }

            var result = await _genericService.DeleteListAsync(ids, ct);
            return result.Match<IActionResult>(
                err => ApiResponse<string>
                    .Fail(["Deletion failed"])
                    .ToBadRequestResult(),
                _ => ApiResponse<object>
                    .Ok(new { DeletedCount = ids.Count() }, "Entities deleted")
                    .ToOkResult()
            );
        }
    }
}
