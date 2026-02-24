using NetTopologySuite.Geometries;

namespace SmartLogistics.Domain.Entities;

public class Order : BaseEntity
{
    public Guid MerchantId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public Point PickupLocation { get; set; } = null!;
    public string DeliveryAddress { get; set; } = string.Empty;
    public Point DeliveryLocation { get; set; } = null!;
    public double WeightKg { get; set; }
    public double VolumeM3 { get; set; }
    public OrderPriority Priority { get; set; } = OrderPriority.Normal;
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public DateTime? Deadline { get; set; }

    // Navigation properties
    public Merchant Merchant { get; set; } = null!;
    public Delivery? Delivery { get; set; }
}

public enum OrderStatus
{
    Created,
    Assigned,
    PickedUp,
    InTransit,
    Delivered,
    Failed,
    Cancelled
}

public enum OrderPriority
{
    Low,
    Normal,
    High,
    Urgent
}