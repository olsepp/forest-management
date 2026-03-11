using App.DAL.EF;
using App.DAL.Repositories.Interfaces;
using App.Domain;
using App.DTO.LandProperty;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories.Implementations;

public class LandPropertyRepository : Repository<LandProperty>, ILandPropertyRepository
{
    public LandPropertyRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<LandProperty?> GetWithCompanyAsync(Guid id)
    {
        return await _dbSet
            .Include(l => l.Company)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<LandProperty>> GetAllWithCompanyAsync()
    {
        return await _dbSet
            .Include(l => l.Company)
            .Include(l => l.Cadasters)
            .ToListAsync();
    }

    public async Task<IEnumerable<LandProperty>> GetAllWithCadastersAsync()
    {
        return await _dbSet
            .Include(l => l.Cadasters)
            .ToListAsync();
    }

    public async Task<IEnumerable<LandProperty>> SearchAsync(LandPropertySearchParams searchParams)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchParams.SearchText))
        {
            query = query.Where(l => l.Name.Contains(searchParams.SearchText));
        }

        if (!string.IsNullOrWhiteSpace(searchParams.County))
        {
            query = query.Where(l => l.County.Contains(searchParams.County));
        }

        if (searchParams.CompanyId.HasValue)
        {
            query = query.Where(l => l.CompanyId == searchParams.CompanyId);
        }

        if (searchParams.Status.HasValue)
        {
            query = query.Where(l => l.Status == searchParams.Status);
        }

        return await query
            .Include(l => l.Company)
            .Include(l => l.Cadasters)
            .ToListAsync();
    }
}
