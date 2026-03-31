using App.BLL.Services.Interfaces;
using App.DTO.LandProperty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("api/[controller]")]
public class LandPropertiesController : ApiControllerBase
{
    private readonly ILandPropertyService _service;

    public LandPropertiesController(ILandPropertyService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LandPropertyListDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LandPropertyDto>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<LandPropertyListDto>>> Search(
        [FromQuery] LandPropertySearchParams searchParams)
    {
        var items = await _service.SearchAsync(searchParams);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LandPropertyDto>> Create([FromBody] LandPropertyCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LandPropertyDto>> Update(Guid id, [FromBody] LandPropertyUpdateDto dto)
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
