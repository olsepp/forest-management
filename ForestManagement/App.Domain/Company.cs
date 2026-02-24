using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class Company : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    public int RegistrationNumber { get; set; }
    
    public ICollection<LandProperty> Properties { get; set; } = new List<LandProperty>();

}