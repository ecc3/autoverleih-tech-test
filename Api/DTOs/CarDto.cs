namespace Api.DTOs;

public record CreateCarRequest(
    string Make,
    string Model,
    string LicensePlate,
    int Year);

public record UpdateCarRequest(
    string Make,
    string Model,
    string LicensePlate,
    int Year);

public record CarResponse(
    Guid Id,
    string Make,
    string Model,
    string LicensePlate,
    int Year,
    bool IsAvailable,
    DateTimeOffset CreatedAt);
