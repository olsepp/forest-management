using App.Contracts.Enums;
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
        var query = BuildSearchQuery(searchParams);

        return await query
            .Include(l => l.Company)
            .Include(l => l.Cadasters)
            .ToListAsync();
    }

    public async Task<(IEnumerable<LandProperty> Items, int Total)> SearchPagedAsync(LandPropertySearchParams searchParams, int skip, int take)
    {
        var query = BuildSearchQuery(searchParams);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(l => l.Status)
            .ThenBy(l => l.Name)
            .Skip(skip)
            .Take(take)
            .Include(l => l.Company)
            .Include(l => l.Cadasters)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<string>> GetDistinctCountiesAsync(Guid companyId)
    {
        return await _dbSet
            .Where(l => l.CompanyId == companyId)
            .Select(l => l.County)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<IEnumerable<LandProperty>> GetSoldByCompanyAsync(Guid companyId)
    {
        return await _dbSet
            .Where(l => l.CompanyId == companyId && l.Status == EPropertyStatus.Sold)
            .Include(l => l.Company)
            .Include(l => l.Cadasters)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    private IQueryable<LandProperty> BuildSearchQuery(LandPropertySearchParams searchParams)
    {
        var query = _dbSet.AsQueryable();

        query = query.Where(l => l.Status != EPropertyStatus.Sold);

        if (!string.IsNullOrWhiteSpace(searchParams.SearchText))
        {
            query = query.Where(l =>
                l.Name.Contains(searchParams.SearchText) ||
                l.RegistrationNumber.ToString().Contains(searchParams.SearchText) ||
                l.Cadasters.Any(c => c.CadastralNumber.Contains(searchParams.SearchText)));
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

        if (searchParams.ActiveOnly)
        {
            query = query.Where(l => l.Status == EPropertyStatus.Active);
        }

        if (searchParams.IsFsc.HasValue)
        {
            query = query.Where(l => l.IsFsc == searchParams.IsFsc);
        }

        return query;
    }
}
