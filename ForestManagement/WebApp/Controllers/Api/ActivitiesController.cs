using App.BLL.Services.Interfaces;
using App.DTO;
using App.DTO.Activity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("api/[controller]")]
public class ActivitiesController : ApiControllerBase
{
    private readonly IActivityService _service;
    private readonly IActivityExportService _exportService;

    public ActivitiesController(IActivityService service, IActivityExportService exportService)
    {
        _service = service;
        _exportService = exportService;
    }

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
    public async Task<ActionResult<PagedResult<ActivityDto>>> GetByCompanyMy(
        Guid companyId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var result = await _service.GetByCompanyAndUserPagedAsync(companyId, userId, skip, take);
        return Ok(result);
    }

    [HttpGet("by-company/{companyId:guid}/filtered")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<ActivityDto>>> GetByCompanyFiltered(
        Guid companyId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? activityTypeId = null,
        [FromQuery] Guid? userId = null)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized();

        var result = await _service.GetByCompanyFilteredPagedAsync(companyId, skip, take, startDate, endDate, activityTypeId, userId);
        return Ok(result);
    }

    [HttpGet("by-company/{companyId:guid}/export")]
    [Authorize(Roles = "Admin")]
    public async Task<FileContentResult> ExportByCompanyFiltered(
        Guid companyId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? activityTypeId = null,
        [FromQuery] Guid? userId = null)
    {
        var excelBytes = await _exportService.ExportActivitiesToExcelAsync(companyId, startDate, endDate, activityTypeId, userId);
        var fileName = $"activities_{companyId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("by-property/{landPropertyId:guid}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByProperty(Guid landPropertyId)
    {
        var items = await _service.GetByLandPropertyIdAsync(landPropertyId);
        return Ok(items);
    }

    [HttpGet("by-property/{landPropertyId:guid}/my")]
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

    [HttpGet("by-user/{userId:guid}/recent")]
    public async Task<ActionResult<IEnumerable<RecentActivityDto>>> GetRecentByUser(
        Guid userId, 
        [FromQuery] int count = 5, 
        [FromQuery] Guid? companyId = null)
    {
        var items = await _service.GetRecentByUserIdAsync(userId, count, companyId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create([FromBody] ActivityCreateDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        try
        {
            var created = await _service.CreateAsync(dto, userId, User.IsInRole("Admin"));
            if (created == null) return NotFound(new { message = "Target user not found." });
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ActivityDto>> Update(Guid id, [FromBody] ActivityUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest(new { message = "Route id does not match body id." });

        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized();

        try
        {
            var updated = await _service.UpdateAsync(id, dto, currentUserId, User.IsInRole("Admin"));
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized();

        var deleted = await _service.DeleteAsync(id, currentUserId, User.IsInRole("Admin"));
        if (!deleted) return NotFound();
        return NoContent();
    }
}
