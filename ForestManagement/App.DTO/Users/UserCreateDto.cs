using System.ComponentModel.DataAnnotations;

namespace App.DTO.Users;

/// <summary>
/// Request DTO for admin to create a new user with temporary password
/// </summary>
public class UserCreateDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty; // "Admin" or "Employee"

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
