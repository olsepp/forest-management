using Microsoft.AspNetCore.Identity;

namespace App.Domain.Identity;

public class AppUserRole : IdentityUserRole<Guid>
{
    public AppUser User { get; set; } = default!;
    public AppRole Role { get; set; } = default!;
}