using App.Domain;

namespace App.DAL.Repositories.Interfaces;

public interface ICompanyRepository : IRepository<Company>
{
    /// <summary>
    /// Get company with all related land properties
    /// </summary>
    Task<Company?> GetWithPropertiesAsync(Guid id);

    /// <summary>
    /// Get all companies with their land properties
    /// </summary>
    Task<IEnumerable<Company>> GetAllWithPropertiesAsync();

    /// <summary>
    /// Get company by registration number
    /// </summary>
    Task<Company?> GetByRegistrationNumberAsync(int registrationNumber);
}
