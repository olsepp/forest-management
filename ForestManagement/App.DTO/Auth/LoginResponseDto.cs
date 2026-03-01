namespace App.DTO.Auth;

/// <summary>
/// Returned after a successful login or registration.
/// </summary>
public class LoginResponseDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // -----------------------------------------------------------------------
    // Access token
    // -----------------------------------------------------------------------
    public string Token { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }

    // -----------------------------------------------------------------------
    // Refresh token
    // -----------------------------------------------------------------------
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}
