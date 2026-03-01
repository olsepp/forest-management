using System.ComponentModel.DataAnnotations;

namespace App.DTO.Users;

/// <summary>
/// Request DTO for updating user details
/// </summary>
public class UserUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
