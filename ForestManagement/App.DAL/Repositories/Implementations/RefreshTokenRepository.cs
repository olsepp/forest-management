using App.DAL.EF;
using App.DAL.Repositories.Interfaces;
using App.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories.Implementations;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);
    }
}
