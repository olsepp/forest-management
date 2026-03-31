using App.BLL.Services.Interfaces;
using App.DTO.ForestStand;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("api/[controller]")]
public class ForestStandsController : ApiControllerBase
{
    private readonly IForestStandService _service;

    public ForestStandsController(IForestStandService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ForestStandListDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ForestStandDto>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("by-cadaster/{cadasterId:guid}")]
    public async Task<ActionResult<IEnumerable<ForestStandListDto>>> GetByCadaster(Guid cadasterId)
    {
        var items = await _service.GetByCadasterIdAsync(cadasterId);
        return Ok(items);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ForestStandListDto>>> GetActive()
    {
        var items = await _service.GetActiveAsync();
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ForestStandDto>> Create([FromBody] ForestStandCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ForestStandDto>> Update(Guid id, [FromBody] ForestStandUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest(new { message = "Route id does not match body id." });

        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
