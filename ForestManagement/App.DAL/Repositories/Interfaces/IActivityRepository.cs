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
}
