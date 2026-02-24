using NetTopologySuite.Geometries;

namespace SmartLogistics.Domain.Entities;

public class Driver : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DriverStatus Status { get; set; } = DriverStatus.Offline;
    public Point? CurrentLocation { get; set; }  // PostGIS geometry
    public double MaxCargoWeightKg { get; set; }
    public double MaxCargoVolumeM3 { get; set; }

    // Navigation properties
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}

public enum DriverStatus
{
    Offline,
    Available,
    OnDelivery,
    OnBreak
}