using Base.Domain;

namespace App.DAL.Repositories.Interfaces;

/// <summary>
/// Base repository interface with common CRUD operations
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Add new entity
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Update existing entity
    /// </summary>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Check if entity exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id);
}
