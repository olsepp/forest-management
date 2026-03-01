using System.ComponentModel.DataAnnotations;
using App.Contracts.Enums;
using Base.Domain;

namespace App.Domain;

public class LandProperty :  BaseEntity
{
    [MaxLength(255)]
    public string Name { get; set; } = null!;
    [Range(0, int.MaxValue)]
    public int RegistrationNumber { get; set; }
    [MaxLength(255)]
    public string County { get; set; } = null!;
    [MaxLength(255)]
    public string Parish { get; set; } = null!;
    [MaxLength(255)]
    public string Village { get; set; } = null!;
    
    public DateTime? BoughtDate { get; set; }
    
    public DateTime? SoldDate { get; set; }
    
    // Foreign keys
    public EPropertyStatus Status { get; set; }
    public Guid CompanyId { get; set; }
    
    // Navigation properties
    public Company Company { get; set; } = null!;
    
    public ICollection<Cadaster> Cadasters { get; set; } = new List<Cadaster>();

}