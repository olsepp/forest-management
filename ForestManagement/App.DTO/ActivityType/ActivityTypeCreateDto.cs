using System.ComponentModel.DataAnnotations;

namespace App.DTO.ActivityType;

/// <summary>
/// Request DTO for creating a new activity type
/// </summary>
public class ActivityTypeCreateDto
{
    [Required]
    [MaxLength(50)]
    public string ActivityTypeName { get; set; } = string.Empty;
}
