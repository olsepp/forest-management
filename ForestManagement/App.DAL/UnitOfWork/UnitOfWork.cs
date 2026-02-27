using App.DAL.EF;
using App.DAL.Repositories.Implementations;
using App.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace App.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private ICompanyRepository? _companies;
    private ILandPropertyRepository? _landProperties;
    private ICadasterRepository? _cadasters;
    private IForestStandRepository? _forestStands;
    private IActivityRepository? _activities;
    private IActivityTypeRepository? _activityTypes;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public ICompanyRepository Companies =>
        _companies ??= new CompanyRepository(_context);

    public ILandPropertyRepository LandProperties =>
        _landProperties ??= new LandPropertyRepository(_context);

    public ICadasterRepository Cadasters =>
        _cadasters ??= new CadasterRepository(_context);

    public IForestStandRepository ForestStands =>
        _forestStands ??= new ForestStandRepository(_context);

    public IActivityRepository Activities =>
        _activities ??= new ActivityRepository(_context);

    public IActivityTypeRepository ActivityTypes =>
        _activityTypes ??= new ActivityTypeRepository(_context);

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
