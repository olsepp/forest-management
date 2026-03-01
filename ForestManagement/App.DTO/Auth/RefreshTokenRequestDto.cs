using System.ComponentModel.DataAnnotations;

namespace App.DTO.Auth;

/// <summary>
/// Request body for POST /api/account/refresh
/// </summary>
public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
