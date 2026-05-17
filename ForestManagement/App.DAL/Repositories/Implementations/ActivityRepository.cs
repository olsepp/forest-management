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

    private IQueryable<Activity> QueryWithDetails()
    {
        return _dbSet
            .Include(a => a.User)
            .Include(a => a.ActivityType)
            .Include(a => a.Cadaster)
                .ThenInclude(c => c!.LandProperty)
                    .ThenInclude(lp => lp.Company)
            .Include(a => a.ForestStand)
                .ThenInclude(fs => fs!.Cadaster)
                    .ThenInclude(c => c.LandProperty)
                        .ThenInclude(lp => lp.Company);
    }

    public async Task<Activity?> GetWithDetailsAsync(Guid id)
    {
        return await QueryWithDetails()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Activity>> GetAllWithDetailsAsync()
    {
        return await QueryWithDetails()
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByForestStandIdAsync(Guid forestStandId)
    {
        return await QueryWithDetails()
            .Where(a => a.ForestStandId == forestStandId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByCadasterIdAsync(Guid cadasterId)
    {
        return await QueryWithDetails()
            .Where(a => a.CadasterId == cadasterId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetRecentAsync(int count)
    {
        return await QueryWithDetails()
            .OrderByDescending(a => a.Date)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByUserIdAsync(Guid userId)
    {
        return await QueryWithDetails()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByCompanyIdAsync(Guid companyId)
    {
        return await QueryWithDetails()
            .Where(a =>
                (a.Cadaster != null && a.Cadaster.LandProperty.CompanyId == companyId) ||
                (a.ForestStand != null && a.ForestStand.Cadaster.LandProperty.CompanyId == companyId))
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByCompanyIdAndUserIdAsync(Guid companyId, Guid userId)
    {
        return await QueryWithDetails()
            .Where(a => a.UserId == userId)
            .Where(a =>
                (a.Cadaster != null && a.Cadaster.LandProperty.CompanyId == companyId) ||
                (a.ForestStand != null && a.ForestStand.Cadaster.LandProperty.CompanyId == companyId))
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByCompanyFilteredAsync(
        Guid companyId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? activityTypeId = null,
        Guid? userId = null)
    {
        var query = QueryWithDetails()
            .Where(a =>
                (a.Cadaster != null && a.Cadaster.LandProperty.CompanyId == companyId) ||
                (a.ForestStand != null && a.ForestStand.Cadaster.LandProperty.CompanyId == companyId));

        if (startDate.HasValue)
            query = query.Where(a => a.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Date <= endDate.Value);

        if (activityTypeId.HasValue)
            query = query.Where(a => a.ActivityTypeId == activityTypeId.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        return await query
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByLandPropertyIdAsync(Guid landPropertyId)
    {
        return await QueryWithDetails()
            .Where(a =>
                (a.Cadaster != null && a.Cadaster.LandPropertyId == landPropertyId) ||
                (a.ForestStand != null && a.ForestStand.Cadaster.LandPropertyId == landPropertyId))
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByLandPropertyIdAndUserIdAsync(Guid landPropertyId, Guid userId)
    {
        return await QueryWithDetails()
            .Where(a => a.UserId == userId)
            .Where(a =>
                (a.Cadaster != null && a.Cadaster.LandPropertyId == landPropertyId) ||
                (a.ForestStand != null && a.ForestStand.Cadaster.LandPropertyId == landPropertyId))
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetRecentByUserIdAsync(Guid userId, int count, Guid? companyId = null)
    {
        var query = QueryWithDetails()
            .Where(a => a.UserId == userId);

        if (companyId.HasValue)
        {
            query = query.Where(a =>
                (a.Cadaster != null && a.Cadaster.LandProperty.CompanyId == companyId.Value) ||
                (a.ForestStand != null && a.ForestStand.Cadaster.LandProperty.CompanyId == companyId.Value));
        }

        return await query
            .OrderByDescending(a => a.Date)
            .Take(count)
            .ToListAsync();
    }
}
