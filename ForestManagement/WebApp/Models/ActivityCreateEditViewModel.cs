using System.ComponentModel.DataAnnotations;
using App.Domain;

namespace WebApp.Models;

public class ActivityCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public string? Notes { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public Guid ActivityTypeId { get; set; }

    // One or the other - mutually exclusive
    public Guid? ForestStandId { get; set; }
    public Guid? CadasterId { get; set; }

    public EApplicationStatus? ApplicationStatus { get; set; }

    // Hidden field - will be set from HttpContext.User
    public Guid UserId { get; set; }
}
