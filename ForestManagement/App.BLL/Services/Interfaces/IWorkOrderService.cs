using App.DTO.WorkOrder;

namespace App.BLL.Services.Interfaces;

public interface IWorkOrderService
{
    Task<WorkOrderDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<WorkOrderListDto>> GetByAssignedUserIdAndCompanyIdAsync(Guid userId, Guid companyId);
    Task<IEnumerable<WorkOrderDto>> GetByCompanyIdAsync(Guid companyId);
    Task<WorkOrderDto> CreateAsync(WorkOrderCreateDto dto, Guid assignedByUserId);
    Task<WorkOrderDto?> UpdateAsync(Guid id, WorkOrderUpdateDto dto);
    Task<WorkOrderDto?> CompleteAsync(Guid id, Guid currentUserId, bool isAdmin);
    Task<WorkOrderDto?> RevertAsync(Guid id, Guid currentUserId, bool isAdmin);
    Task<bool> DeleteAsync(Guid id);
}
