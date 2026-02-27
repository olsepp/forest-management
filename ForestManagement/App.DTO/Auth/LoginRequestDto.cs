using System.ComponentModel.DataAnnotations;

namespace App.DTO.Auth;

/// <summary>
/// Request DTO for user login
/// </summary>
public class LoginRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}
