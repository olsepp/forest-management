using App.Domain;

namespace App.DAL.Repositories.Interfaces;

public interface ICadasterRepository : IRepository<Cadaster>
{
    /// <summary>
    /// Get cadaster with land property
    /// </summary>
    Task<Cadaster?> GetWithLandPropertyAsync(Guid id);

    /// <summary>
    /// Get all cadasters with forest stands
    /// </summary>
    Task<IEnumerable<Cadaster>> GetAllWithForestStandsAsync();

    /// <summary>
    /// Get cadaster by cadastral number
    /// </summary>
    Task<Cadaster?> GetByCadastralNumberAsync(string cadastralNumber);

    /// <summary>
    /// Get cadasters by land property ID
    /// </summary>
    Task<IEnumerable<Cadaster>> GetByLandPropertyIdAsync(Guid landPropertyId);
}
