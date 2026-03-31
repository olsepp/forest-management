using App.BLL.Services.Interfaces;
using App.DTO.Activity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("api/[controller]")]
public class ActivitiesController : ApiControllerBase
{
    private readonly IActivityService _service;

    public ActivitiesController(IActivityService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityListDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ActivityDto>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("by-foreststand/{forestStandId:guid}")]
    public async Task<ActionResult<IEnumerable<ActivityListDto>>> GetByForestStand(Guid forestStandId)
    {
        var items = await _service.GetByForestStandIdAsync(forestStandId);
        return Ok(items);
    }

    [HttpGet("by-cadaster/{cadasterId:guid}")]
    public async Task<ActionResult<IEnumerable<ActivityListDto>>> GetByCadaster(Guid cadasterId)
    {
        var items = await _service.GetByCadasterIdAsync(cadasterId);
        return Ok(items);
    }

    [HttpGet("by-company/{companyId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByCompany(Guid companyId)
    {
        var items = await _service.GetByCompanyIdAsync(companyId);
        return Ok(items);
    }

    [HttpGet("by-company/{companyId:guid}/my")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByCompanyMy(Guid companyId)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var items = await _service.GetByCompanyIdAndUserIdAsync(companyId, userId);
        return Ok(items);
    }

    [HttpGet("by-property/{landPropertyId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByProperty(Guid landPropertyId)
    {
        var items = await _service.GetByLandPropertyIdAsync(landPropertyId);
        return Ok(items);
    }

    [HttpGet("by-property/{landPropertyId:guid}/my")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByPropertyMy(Guid landPropertyId)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var items = await _service.GetByLandPropertyIdAndUserIdAsync(landPropertyId, userId);
        return Ok(items);
    }

    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<RecentActivityDto>>> GetRecent([FromQuery] int count = 10)
    {
        var items = await _service.GetRecentAsync(count);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<ActionResult<ActivityDto>> Create([FromBody] ActivityCreateDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var created = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<ActionResult<ActivityDto>> Update(Guid id, [FromBody] ActivityUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest(new { message = "Route id does not match body id." });

        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized();

        var updated = await _service.UpdateAsync(id, dto, currentUserId, User.IsInRole("Admin"));
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized();

        var deleted = await _service.DeleteAsync(id, currentUserId, User.IsInRole("Admin"));
        if (!deleted) return NotFound();
        return NoContent();
    }
}
