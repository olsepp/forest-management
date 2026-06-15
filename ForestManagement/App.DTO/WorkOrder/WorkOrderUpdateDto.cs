using System.ComponentModel.DataAnnotations;

namespace App.DTO.WorkOrder;

public class WorkOrderUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid AssignedToId { get; set; }

    [Required]
    public Guid ActivityTypeId { get; set; }

    [Required]
    public Guid CadasterId { get; set; }

    public Guid? ForestStandId { get; set; }

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public string? Notes { get; set; }
}
