using App.Domain;

namespace App.DAL.Repositories.Interfaces;

public interface IForestStandRepository : IRepository<ForestStand>
{
    /// <summary>
    /// Get forest stand with cadaster
    /// </summary>
    Task<ForestStand?> GetWithCadasterAsync(Guid id);

    /// <summary>
    /// Get all forest stands with cadaster
    /// </summary>
    Task<IEnumerable<ForestStand>> GetAllWithCadasterAsync();

    /// <summary>
    /// Get forest stands by cadaster ID
    /// </summary>
    Task<IEnumerable<ForestStand>> GetByCadasterIdAsync(Guid cadasterId);

    /// <summary>
    /// Get active forest stands
    /// </summary>
    Task<IEnumerable<ForestStand>> GetActiveAsync();
}
