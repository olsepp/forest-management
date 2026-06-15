using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class ActivityType : BaseEntity
{
    [MaxLength(50)]
    public string ActivityTypeName { get; set; } = null!;
    
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    
    
    /*
     * Istutamine
     * Maapinna mineraliseerimine
     * Valgustusraie
     * Raie
     * Kokkuvedu
     * Lageraie
     * Aegjargne raie
     * sanitar raie
     * Metsamaterjali kokkuvedu
     * Raiejaatmete kokkuvedu
     * Raielangi ettevalmistus(vosa loikus)
     */
}