namespace App.DTO.ActivityType;

/// <summary>
/// Activity type list item DTO (Id + name only)
/// </summary>
public class ActivityTypeListDto
{
    public Guid Id { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
}
