namespace App.DTO.Users;

/// <summary>
/// Table display DTO for user list with roles
/// </summary>
public class UserListDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
