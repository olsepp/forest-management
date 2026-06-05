using App.BLL.Services.Interfaces;
using App.DTO;
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

    [HttpGet("search-paged")]
    public async Task<ActionResult<PagedResult<LandPropertyListDto>>> SearchPaged(
        [FromQuery] LandPropertySearchParams searchParams,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var result = await _service.SearchPagedAsync(searchParams, skip, take);
        return Ok(result);
    }

    [HttpGet("counties")]
    public async Task<ActionResult<IEnumerable<string>>> GetCounties([FromQuery] Guid companyId)
    {
        var counties = await _service.GetDistinctCountiesAsync(companyId);
        return Ok(counties);
    }

    [HttpGet("sold")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<LandPropertyListDto>>> GetSold([FromQuery] Guid companyId)
    {
        var items = await _service.GetSoldByCompanyAsync(companyId);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LandPropertyDto>> Create([FromBody] LandPropertyCreateDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LandPropertyDto>> Update(Guid id, [FromBody] LandPropertyUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest(new { message = "Route id does not match body id." });

        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
