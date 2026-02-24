namespace SmartLogistics.Domain.Entities;

/// <summary>
/// Represents a weighted, directed edge between two zones.
/// This is the graph edge for routing algorithms.
/// </summary>
public class ZoneConnection : BaseEntity
{
    public Guid FromZoneId { get; set; }
    public Guid ToZoneId { get; set; }
    public double DistanceKm { get; set; }
    public double EstimatedTimeMinutes { get; set; }
    public double TrafficMultiplier { get; set; } = 1.0;

    // Navigation properties
    public Zone FromZone { get; set; } = null!;
    public Zone ToZone { get; set; } = null!;
}