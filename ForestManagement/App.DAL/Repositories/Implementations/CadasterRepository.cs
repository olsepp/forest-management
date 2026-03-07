using App.DAL.EF;
using App.DAL.Repositories.Interfaces;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories.Implementations;

public class CadasterRepository : Repository<Cadaster>, ICadasterRepository
{
    public CadasterRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Cadaster?> GetWithLandPropertyAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.LandProperty)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Cadaster>> GetAllWithForestStandsAsync()
    {
        return await _dbSet
            .Include(c => c.ForestStands)
            .ToListAsync();
    }

    public async Task<Cadaster?> GetByCadastralNumberAsync(string cadastralNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.CadastralNumber == cadastralNumber);
    }

    public async Task<IEnumerable<Cadaster>> GetByLandPropertyIdAsync(Guid landPropertyId)
    {
        return await _dbSet
            .Where(c => c.LandPropertyId == landPropertyId)
            .ToListAsync();
    }

    public async Task<Cadaster?> GetWithAllAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.LandProperty)
            .Include(c => c.ForestStands)
            .Include(c => c.Activities)
                .ThenInclude(a => a.ActivityType)
            .Include(c => c.Activities)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
