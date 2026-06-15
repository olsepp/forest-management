using App.BLL.Services.Interfaces;
using App.DTO.WorkOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

[Route("api/[controller]")]
public class WorkOrdersController : ApiControllerBase
{
    private readonly IWorkOrderService _service;

    public WorkOrdersController(IWorkOrderService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkOrderDto>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("by-company/{companyId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetByCompany(Guid companyId)
    {
        var items = await _service.GetByCompanyIdAsync(companyId);
        return Ok(items);
    }

    [HttpGet("by-company/{companyId:guid}/my")]
    public async Task<ActionResult<IEnumerable<WorkOrderListDto>>> GetByCompanyMy(Guid companyId)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var items = await _service.GetByAssignedUserIdAndCompanyIdAsync(userId, companyId);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WorkOrderDto>> Create([FromBody] WorkOrderCreateDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var created = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WorkOrderDto>> Update(Guid id, [FromBody] WorkOrderUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest(new { message = "Route id does not match body id." });

        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<WorkOrderDto>> Complete(Guid id)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized();

        var completed = await _service.CompleteAsync(id, currentUserId, User.IsInRole("Admin"));
        if (completed == null) return NotFound();
        return Ok(completed);
    }

    [HttpPost("{id:guid}/revert")]
    public async Task<ActionResult<WorkOrderDto>> Revert(Guid id)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Unauthorized();

        var reverted = await _service.RevertAsync(id, currentUserId, User.IsInRole("Admin"));
        if (reverted == null) return NotFound();
        return Ok(reverted);
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
