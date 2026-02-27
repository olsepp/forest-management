namespace App.DTO.ActivityType;

/// <summary>
/// Full activity type details response DTO
/// </summary>
public class ActivityTypeDto
{
    public Guid Id { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
    public int ActivityCount { get; set; }
}
