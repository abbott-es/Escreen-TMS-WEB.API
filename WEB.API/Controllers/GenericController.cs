using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.IService;

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
        public async Task<ActionResult<T>> GetById(Guid id, CancellationToken ct = default)
        {
            var result = await _genericService.GetByIdAsync(id, ct);
            return result.Match<ActionResult>(
                Left: error => NotFound(new { Error = error }),
                Right: success => Ok(success)
            );
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<T>>> GetAll(CancellationToken ct = default)
        {
            var result = await _genericService.GetAllAsync(ct);
            return result.Match<ActionResult>(
                Left: error => NotFound(new { Error = error }),
                Right: success => Ok(success)
            );
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] T entity, CancellationToken ct = default)
        {
            var result = await _genericService.AddAsync(entity, ct);
            return result.Match<IActionResult>(
                Left: error => BadRequest(new { Error = error }),
                Right: id => CreatedAtAction(nameof(GetById), new { id }, entity)
            );
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] T entity, CancellationToken ct = default)
        {
            var result = await _genericService.UpdateAsync(entity, ct);
            return result.Match<IActionResult>(
                Left: error => NotFound(new { Error = error }),
                Right: success => NoContent()
            );
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteListAsync([FromBody] IEnumerable<Guid> ids, CancellationToken ct)
        {
            if (ids == null || !ids.Any())
                return BadRequest("No IDs provided.");

            var result = await _genericService.DeleteListAsync(ids, ct);

            return result.Match<IActionResult>(
                err => BadRequest(new { Error = err }),
                _ => Ok(new { DeletedCount = ids.Count() })
            );
        }
    }
}
