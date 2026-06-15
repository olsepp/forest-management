using App.BLL.Services.Interfaces;
using App.Contracts.Enums;
using App.DAL.UnitOfWork;
using App.Domain;
using App.DTO.WorkOrder;

namespace App.BLL.Services.Implementations;

public class WorkOrderService : IWorkOrderService
{
    private readonly IUnitOfWork _uow;

    public WorkOrderService(IUnitOfWork uow) => _uow = uow;

    public async Task<WorkOrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _uow.WorkOrders.GetWithDetailsAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<WorkOrderListDto>> GetByAssignedUserIdAndCompanyIdAsync(Guid userId, Guid companyId)
    {
        var orders = await _uow.WorkOrders.GetByAssignedUserIdAndCompanyIdAsync(userId, companyId);
        return orders.Select(MapToListDto);
    }

    public async Task<IEnumerable<WorkOrderDto>> GetByCompanyIdAsync(Guid companyId)
    {
        var orders = await _uow.WorkOrders.GetByCompanyIdAsync(companyId);
        return orders.Select(MapToDto);
    }

    public async Task<WorkOrderDto> CreateAsync(WorkOrderCreateDto dto, Guid createdByUserId)
    {
        var entity = new WorkOrder
        {
            AssignedToId = dto.AssignedToId,
            AssignedById = createdByUserId,
            ActivityTypeId = dto.ActivityTypeId,
            CadasterId = dto.CadasterId,
            ForestStandId = dto.ForestStandId,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Notes = dto.Notes,
            Status = EOrderStatus.Sent,
            CreatedAt = DateTime.UtcNow
        };
        await _uow.WorkOrders.AddAsync(entity);
        await _uow.SaveChangesAsync();
        var created = await _uow.WorkOrders.GetWithDetailsAsync(entity.Id);
        return MapToDto(created!);
    }

    public async Task<WorkOrderDto?> UpdateAsync(Guid id, WorkOrderUpdateDto dto)
    {
        var entity = await _uow.WorkOrders.GetByIdAsync(id);
        if (entity == null) return null;

        entity.AssignedToId = dto.AssignedToId;
        entity.ActivityTypeId = dto.ActivityTypeId;
        entity.CadasterId = dto.CadasterId;
        entity.ForestStandId = dto.ForestStandId;
        entity.Quantity = dto.Quantity;
        entity.Unit = dto.Unit;
        entity.Notes = dto.Notes;

        await _uow.WorkOrders.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        var updated = await _uow.WorkOrders.GetWithDetailsAsync(id);
        return MapToDto(updated!);
    }

    public async Task<WorkOrderDto?> CompleteAsync(Guid id, Guid currentUserId, bool isAdmin)
    {
        var entity = await _uow.WorkOrders.GetByIdAsync(id);
        if (entity == null) return null;

        if (!isAdmin && entity.AssignedToId != currentUserId)
            return null;

        entity.Status = EOrderStatus.Completed;
        await _uow.WorkOrders.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        var updated = await _uow.WorkOrders.GetWithDetailsAsync(id);
        return MapToDto(updated!);
    }

    public async Task<WorkOrderDto?> RevertAsync(Guid id, Guid currentUserId, bool isAdmin)
    {
        var entity = await _uow.WorkOrders.GetByIdAsync(id);
        if (entity == null) return null;

        if (!isAdmin && entity.AssignedToId != currentUserId)
            return null;

        entity.Status = EOrderStatus.Sent;
        await _uow.WorkOrders.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
        var updated = await _uow.WorkOrders.GetWithDetailsAsync(id);
        return MapToDto(updated!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _uow.WorkOrders.ExistsAsync(id)) return false;

        await _uow.WorkOrders.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return true;
    }

    private static WorkOrderDto MapToDto(WorkOrder w) => new()
    {
        Id = w.Id,
        AssignedToId = w.AssignedToId,
        AssignedToUserName = w.AssignedTo?.UserName ?? string.Empty,
        AssignedById = w.AssignedById,
        AssignedByUserName = w.AssignedBy?.UserName ?? string.Empty,
        ActivityTypeId = w.ActivityTypeId,
        ActivityTypeName = w.ActivityType?.ActivityTypeName ?? string.Empty,
        ForestStandId = w.ForestStandId,
        ForestStandNumber = w.ForestStand?.Number,
        CadasterId = w.CadasterId,
        CadasterCadastralNumber = w.Cadaster?.CadastralNumber ?? string.Empty,
        Status = w.Status,
        Quantity = w.Quantity,
        Unit = w.Unit,
        Notes = w.Notes,
        CreatedAt = w.CreatedAt
    };

    private static WorkOrderListDto MapToListDto(WorkOrder w) => new()
    {
        Id = w.Id,
        AssignedToUserName = w.AssignedTo?.UserName ?? string.Empty,
        ActivityTypeName = w.ActivityType?.ActivityTypeName ?? string.Empty,
        CadasterCadastralNumber = w.Cadaster?.CadastralNumber ?? string.Empty,
        CadasterId = w.CadasterId,
        ForestStandNumber = w.ForestStand?.Number,
        ForestStandId = w.ForestStandId,
        Status = w.Status,
        Quantity = w.Quantity,
        Unit = w.Unit,
        CreatedAt = w.CreatedAt
    };
}
