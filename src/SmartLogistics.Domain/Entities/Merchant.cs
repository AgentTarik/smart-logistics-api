using NetTopologySuite.Geometries;

namespace SmartLogistics.Domain.Entities;

public class Merchant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiKey { get; set; } = Guid.NewGuid().ToString("N");
    public string Address { get; set; } = string.Empty;
    public Point Location { get; set; } = null!;  // PostGIS geometry

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}