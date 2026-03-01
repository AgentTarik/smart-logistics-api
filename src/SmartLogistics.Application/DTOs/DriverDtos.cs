namespace SmartLogistics.Application.DTOs;

public record CreateDriverRequest(
    string Name,
    string Email,
    string PhoneNumber,
    string LicensePlate,
    double MaxCargoWeightKg,
    double MaxCargoVolumeM3
);

public record UpdateDriverRequest(
    string Name,
    string PhoneNumber,
    string LicensePlate,
    double MaxCargoWeightKg,
    double MaxCargoVolumeM3
);

public record DriverResponse(
    Guid Id,
    string Name,
    string Email,
    string PhoneNumber,
    string LicensePlate,
    string Status,
    double? Latitude,
    double? Longitude,
    double MaxCargoWeightKg,
    double MaxCargoVolumeM3,
    DateTime CreatedAt
);

public record UpdateDriverLocationRequest(
    double Latitude,
    double Longitude
);