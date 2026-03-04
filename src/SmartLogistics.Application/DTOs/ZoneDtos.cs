namespace SmartLogistics.Application.DTOs;

public record CreateZoneRequest(
    string Name,
    double BaseDeliveryCost,
    double[][] BoundaryCoordinates // Array of [longitude, latitude] pairs forming the polygon
);

public record UpdateZoneRequest(
    string Name,
    double BaseDeliveryCost,
    double[][]? BoundaryCoordinates
);

public record ZoneResponse(
    Guid Id,
    string Name,
    double BaseDeliveryCost,
    DateTime CreatedAt
);
