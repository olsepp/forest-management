using App.Domain.Identity;

namespace App.DAL.Repositories.Interfaces;

/// <summary>
/// Repository for persisting and retrieving refresh tokens.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Load a refresh token by its SHA-256 hash, including the owning <see cref="AppUser"/>.
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
}
