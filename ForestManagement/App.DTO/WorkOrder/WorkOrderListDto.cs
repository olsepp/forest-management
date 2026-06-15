using App.Contracts.Enums;

namespace App.DTO.WorkOrder;

public class WorkOrderListDto
{
    public Guid Id { get; set; }

    public string AssignedToUserName { get; set; } = string.Empty;

    public string ActivityTypeName { get; set; } = string.Empty;

    public string CadasterCadastralNumber { get; set; } = string.Empty;

    public Guid CadasterId { get; set; }

    public int? ForestStandNumber { get; set; }

    public Guid? ForestStandId { get; set; }

    public EOrderStatus Status { get; set; }

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public DateTime CreatedAt { get; set; }
}
