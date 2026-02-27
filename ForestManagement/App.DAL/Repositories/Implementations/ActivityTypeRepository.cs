using App.DAL.EF;
using App.DAL.Repositories.Interfaces;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories.Implementations;

public class ActivityTypeRepository : Repository<ActivityType>, IActivityTypeRepository
{
    public ActivityTypeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ActivityType?> GetWithActivitiesAsync(Guid id)
    {
        return await _dbSet
            .Include(a => a.Activities)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}
