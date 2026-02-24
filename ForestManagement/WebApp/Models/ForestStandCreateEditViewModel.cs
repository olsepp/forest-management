using System.ComponentModel.DataAnnotations;
using App.Domain;

namespace WebApp.Models;

public class ForestStandCreateEditViewModel
{
    public Guid Id { get; set; }

    [Range(0, 100)]
    public int Number { get; set; }

    public decimal Area { get; set; }

    public int TotalVolume { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    public DateTime? ValidTo { get; set; }

    [Required]
    public Guid CadasterId { get; set; }
}
