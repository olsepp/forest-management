using App.DTO.Auth;

namespace App.BLL.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
    Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto dto);

    /// <summary>
    /// Exchange a valid refresh token for a new access token + rotated refresh token.
    /// Returns <c>null</c> if the token is invalid, expired, or already revoked.
    /// </summary>
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revoke the supplied refresh token (logout).
    /// Returns <c>false</c> if the token was not found or is already inactive.
    /// </summary>
    Task<bool> LogoutAsync(string refreshToken);
}
