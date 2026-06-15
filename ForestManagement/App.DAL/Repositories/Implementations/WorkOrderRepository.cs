using App.DAL.EF;
using App.DAL.Repositories.Interfaces;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories.Implementations;

public class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(AppDbContext context) : base(context)
    {
    }

    private IQueryable<WorkOrder> QueryWithDetails()
    {
        return _dbSet
            .Include(w => w.AssignedTo)
            .Include(w => w.AssignedBy)
            .Include(w => w.ActivityType)
            .Include(w => w.Cadaster)
                .ThenInclude(c => c.LandProperty)
                    .ThenInclude(lp => lp.Company)
            .Include(w => w.ForestStand!)
                .ThenInclude(fs => fs.Cadaster)
                    .ThenInclude(c => c.LandProperty)
                        .ThenInclude(lp => lp.Company);
    }

    public async Task<WorkOrder?> GetWithDetailsAsync(Guid id)
    {
        return await QueryWithDetails()
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<WorkOrder>> GetByAssignedUserIdAndCompanyIdAsync(Guid userId, Guid companyId)
    {
        return await QueryWithDetails()
            .Where(w => w.AssignedToId == userId
                        && w.Cadaster.LandProperty.CompanyId == companyId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkOrder>> GetByCompanyIdAsync(Guid companyId)
    {
        return await QueryWithDetails()
            .Where(w => w.Cadaster.LandProperty.CompanyId == companyId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }
}
