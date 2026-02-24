using System.ComponentModel.DataAnnotations;
using App.Domain;

namespace WebApp.Models;

public class LandPropertyCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int RegistrationNumber { get; set; }

    [Required]
    [MaxLength(255)]
    public string County { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Parish { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Village { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? BoughtDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? SoldDate { get; set; }

    [Required]
    public EPropertyStatus Status { get; set; }

    [Required]
    public Guid CompanyId { get; set; }
}
