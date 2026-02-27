using App.Domain;
using App.DTO.LandProperty;

namespace App.DAL.Repositories.Interfaces;

public interface ILandPropertyRepository : IRepository<LandProperty>
{
    /// <summary>
    /// Get land property with company
    /// </summary>
    Task<LandProperty?> GetWithCompanyAsync(Guid id);

    /// <summary>
    /// Get all land properties with company
    /// </summary>
    Task<IEnumerable<LandProperty>> GetAllWithCompanyAsync();

    /// <summary>
    /// Get land properties with cadasters
    /// </summary>
    Task<IEnumerable<LandProperty>> GetAllWithCadastersAsync();

    /// <summary>
    /// Search land properties by criteria
    /// </summary>
    Task<IEnumerable<LandProperty>> SearchAsync(LandPropertySearchParams searchParams);
}
