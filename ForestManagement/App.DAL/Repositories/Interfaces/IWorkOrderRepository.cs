using App.Domain;

namespace App.DAL.Repositories.Interfaces;

public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    Task<WorkOrder?> GetWithDetailsAsync(Guid id);
    Task<IEnumerable<WorkOrder>> GetByAssignedUserIdAndCompanyIdAsync(Guid userId, Guid companyId);
    Task<IEnumerable<WorkOrder>> GetByCompanyIdAsync(Guid companyId);
}
