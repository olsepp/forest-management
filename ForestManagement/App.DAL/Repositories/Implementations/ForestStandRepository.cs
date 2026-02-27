using App.DAL.EF;
using App.DAL.Repositories.Interfaces;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories.Implementations;

public class ForestStandRepository : Repository<ForestStand>, IForestStandRepository
{
    public ForestStandRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ForestStand?> GetWithCadasterAsync(Guid id)
    {
        return await _dbSet
            .Include(f => f.Cadaster)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<ForestStand>> GetAllWithCadasterAsync()
    {
        return await _dbSet
            .Include(f => f.Cadaster)
            .ToListAsync();
    }

    public async Task<IEnumerable<ForestStand>> GetByCadasterIdAsync(Guid cadasterId)
    {
        return await _dbSet
            .Where(f => f.CadasterId == cadasterId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ForestStand>> GetActiveAsync()
    {
        return await _dbSet
            .Where(f => f.IsActive)
            .Include(f => f.Cadaster)
            .ToListAsync();
    }
}
