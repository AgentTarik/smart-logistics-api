namespace SmartLogistics.Domain.Entities;

public class Delivery : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid DriverId { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public double? DistanceKm { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
}

public enum DeliveryStatus
{
    Pending,
    DriverEnRoute,
    PickedUp,
    InTransit,
    Delivered,
    Failed
}