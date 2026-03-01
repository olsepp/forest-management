using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.BLL.Services.Interfaces;
using App.DAL.UnitOfWork;
using App.Domain.Identity;
using App.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace App.BLL.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _uow;
    private readonly JwtSettings _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<AppUser> userManager,
        IUnitOfWork uow,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _uow = uow;
        _jwt = jwtOptions.Value;
        _logger = logger;
    }

    // -----------------------------------------------------------------------
    // Public methods
    // -----------------------------------------------------------------------

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        // Support login by username or email
        var user = await _userManager.FindByNameAsync(dto.Username)
                   ?? await _userManager.FindByEmailAsync(dto.Username);

        if (user == null)
        {
            _logger.LogWarning("Login failed: user '{Username}' not found", dto.Username);
            return null;
        }

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _logger.LogWarning("Login failed: invalid password for user '{Username}'", dto.Username);
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAuthResponseAsync(user, roles.FirstOrDefault() ?? string.Empty);
    }

    public async Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto dto)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for '{Username}': {Errors}", dto.Username, errors);
            return null;
        }

        return await BuildAuthResponseAsync(user, string.Empty);
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _uow.RefreshTokens.GetByTokenAsync(refreshToken);

        if (stored == null || !stored.IsActive)
        {
            _logger.LogWarning("Refresh attempt with an invalid/expired/revoked token");
            return null;
        }

        // Revoke the old token immediately (token rotation)
        stored.RevokedAt = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(stored.User);
        var response = await BuildAuthResponseAsync(stored.User, roles.FirstOrDefault() ?? string.Empty);

        // SaveChangesAsync is called inside BuildAuthResponseAsync (when it inserts the new refresh token).
        // The revocation of the old token is also flushed at that point because the DbContext tracks it.
        return response;
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var stored = await _uow.RefreshTokens.GetByTokenAsync(refreshToken);

        if (stored == null || !stored.IsActive)
            return false;

        stored.RevokedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private async Task<LoginResponseDto> BuildAuthResponseAsync(AppUser user, string role)
    {
        var (accessToken, tokenExpiry) = GenerateAccessToken(user, role);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = role,
            Token = accessToken,
            TokenExpiresAt = tokenExpiry,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt
        };
    }

    private (string token, DateTime expiresAt) GenerateAccessToken(AppUser user, string role)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpiresInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(role))
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private async Task<RefreshToken> GenerateAndStoreRefreshTokenAsync(Guid userId)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(tokenBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiresInDays),
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        await _uow.RefreshTokens.AddAsync(refreshToken);
        await _uow.SaveChangesAsync();
        return refreshToken;
    }
}
