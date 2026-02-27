using App.DAL.EF;
using App.DAL.Repositories.Interfaces;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories.Implementations;

public class ActivityRepository : Repository<Activity>, IActivityRepository
{
    public ActivityRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Activity?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.ActivityType)
            .Include(a => a.ForestStand)
            .Include(a => a.Cadaster)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Activity>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.ActivityType)
            .Include(a => a.ForestStand)
            .Include(a => a.Cadaster)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByForestStandIdAsync(Guid forestStandId)
    {
        return await _dbSet
            .Where(a => a.ForestStandId == forestStandId)
            .Include(a => a.User)
            .Include(a => a.ActivityType)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByCadasterIdAsync(Guid cadasterId)
    {
        return await _dbSet
            .Where(a => a.CadasterId == cadasterId)
            .Include(a => a.User)
            .Include(a => a.ActivityType)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetRecentAsync(int count)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.ActivityType)
            .OrderByDescending(a => a.Date)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .Include(a => a.ActivityType)
            .Include(a => a.ForestStand)
            .Include(a => a.Cadaster)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }
}
