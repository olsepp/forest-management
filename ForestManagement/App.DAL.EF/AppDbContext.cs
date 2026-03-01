using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, AppRole, Guid>(options)
{
    public DbSet<Activity> Activities { get; set; }
    public DbSet<ActivityType> ActivityTypes { get; set; }
    public DbSet<Cadaster> Cadasters { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<ForestStand> ForestStands { get; set; }
    public DbSet<LandProperty> LandProperties { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}