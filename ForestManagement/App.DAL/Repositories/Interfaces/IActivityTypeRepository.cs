using App.Domain;

namespace App.DAL.Repositories.Interfaces;

public interface IActivityTypeRepository : IRepository<ActivityType>
{
    /// <summary>
    /// Get activity type with activities
    /// </summary>
    Task<ActivityType?> GetWithActivitiesAsync(Guid id);
}
