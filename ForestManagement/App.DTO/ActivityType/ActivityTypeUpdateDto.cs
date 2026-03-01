using System.ComponentModel.DataAnnotations;

namespace App.DTO.ActivityType;

/// <summary>
/// Request DTO for updating an existing activity type
/// </summary>
public class ActivityTypeUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActivityTypeName { get; set; } = string.Empty;
}
