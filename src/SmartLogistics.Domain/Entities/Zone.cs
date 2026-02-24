using NetTopologySuite.Geometries;

namespace SmartLogistics.Domain.Entities;

public class Zone : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Polygon Boundary { get; set; } = null!;  // PostGIS polygon
    public double BaseDeliveryCost { get; set; }

    // Navigation properties
    public ICollection<ZoneConnection> OutgoingConnections { get; set; } = new List<ZoneConnection>();
    public ICollection<ZoneConnection> IncomingConnections { get; set; } = new List<ZoneConnection>();
}