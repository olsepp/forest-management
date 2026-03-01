using App.DAL.Repositories.Interfaces;

namespace App.DAL.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    ICompanyRepository Companies { get; }
    ILandPropertyRepository LandProperties { get; }
    ICadasterRepository Cadasters { get; }
    IForestStandRepository ForestStands { get; }
    IActivityRepository Activities { get; }
    IActivityTypeRepository ActivityTypes { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    /// <summary>
    /// Save all changes
    /// </summary>
    Task SaveChangesAsync();

    /// <summary>
    /// Begin a new transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
}
