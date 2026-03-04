namespace SmartLogistics.Application.DTOs;

public record CreateMerchantRequest(
    string Name,
    string Email,
    string Address,
    double Latitude,
    double Longitude
);

public record UpdateMerchantRequest(
    string Name,
    string Address,
    double Latitude,
    double Longitude
);

public record MerchantResponse(
    Guid Id,
    string Name,
    string Email,
    string ApiKey,
    string Address,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt
);
