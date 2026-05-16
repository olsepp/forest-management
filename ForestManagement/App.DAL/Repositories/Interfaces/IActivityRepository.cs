using App.Domain;

namespace App.DAL.Repositories.Interfaces;

public interface IActivityRepository : IRepository<Activity>
{
    /// <summary>
    /// Get activity with all related entities
    /// </summary>
    Task<Activity?> GetWithDetailsAsync(Guid id);

    /// <summary>
    /// Get all activities with details
    /// </summary>
    Task<IEnumerable<Activity>> GetAllWithDetailsAsync();

    /// <summary>
    /// Get activities by forest stand ID
    /// </summary>
    Task<IEnumerable<Activity>> GetByForestStandIdAsync(Guid forestStandId);

    /// <summary>
    /// Get activities by cadaster ID
    /// </summary>
    Task<IEnumerable<Activity>> GetByCadasterIdAsync(Guid cadasterId);

    /// <summary>
    /// Get recent activities
    /// </summary>
    Task<IEnumerable<Activity>> GetRecentAsync(int count);

    /// <summary>
    /// Get activities by user ID
    /// </summary>
    Task<IEnumerable<Activity>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Get activities by company ID
    /// </summary>
    Task<IEnumerable<Activity>> GetByCompanyIdAsync(Guid companyId);

    /// <summary>
    /// Get activities by company ID and user ID
    /// </summary>
    Task<IEnumerable<Activity>> GetByCompanyIdAndUserIdAsync(Guid companyId, Guid userId);

    /// <summary>
    /// Get activities by company ID and date range
    /// </summary>
    Task<IEnumerable<Activity>> GetByCompanyIdAndDateRangeAsync(Guid companyId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get activities by land property ID
    /// </summary>
    Task<IEnumerable<Activity>> GetByLandPropertyIdAsync(Guid landPropertyId);

    /// <summary>
    /// Get activities by land property ID and user ID
    /// </summary>
    Task<IEnumerable<Activity>> GetByLandPropertyIdAndUserIdAsync(Guid landPropertyId, Guid userId);

    /// <summary>
    /// Get recent activities by user ID, optionally filtered by company ID
    /// </summary>
    Task<IEnumerable<Activity>> GetRecentByUserIdAsync(Guid userId, int count, Guid? companyId = null);
}
