using App.BLL.Services.Interfaces;
using App.DTO.Cadaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("api/[controller]")]
public class CadastersController : ApiControllerBase
{
    private readonly ICadasterService _service;

    public CadastersController(ICadasterService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CadasterListDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CadasterDto>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("by-land-property/{landPropertyId:guid}")]
    public async Task<ActionResult<IEnumerable<CadasterListDto>>> GetByLandProperty(Guid landPropertyId)
    {
        var items = await _service.GetByLandPropertyIdAsync(landPropertyId);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CadasterDto>> Create([FromBody] CadasterCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CadasterDto>> Update(Guid id, [FromBody] CadasterUpdateDto dto)
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
