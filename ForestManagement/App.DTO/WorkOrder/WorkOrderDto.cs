using App.Contracts.Enums;

namespace App.DTO.WorkOrder;

public class WorkOrderDto
{
    public Guid Id { get; set; }

    public Guid AssignedToId { get; set; }
    public string AssignedToUserName { get; set; } = string.Empty;

    public Guid AssignedById { get; set; }
    public string AssignedByUserName { get; set; } = string.Empty;

    public Guid ActivityTypeId { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;

    public Guid? ForestStandId { get; set; }
    public int? ForestStandNumber { get; set; }

    public Guid CadasterId { get; set; }
    public string CadasterCadastralNumber { get; set; } = string.Empty;

    public EOrderStatus Status { get; set; }

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
