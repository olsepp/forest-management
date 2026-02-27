using App.DAL.EF;
using App.DAL.Repositories.Interfaces;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories.Implementations;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Company?> GetWithPropertiesAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Properties)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Company>> GetAllWithPropertiesAsync()
    {
        return await _dbSet
            .Include(c => c.Properties)
            .ToListAsync();
    }

    public async Task<Company?> GetByRegistrationNumberAsync(int registrationNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber);
    }
}
