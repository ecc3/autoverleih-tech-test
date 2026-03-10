namespace Api.DTOs;

public record CreateRentalRequest(
    Guid CustomerId,
    Guid CarId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);

public record ReturnRentalRequest(int KilometersDriven);

public record RentalResponse(
    Guid Id,
    Guid CustomerId,
    Guid CarId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    DateTimeOffset? ReturnedAt,
    string Status,
    int? KilometersDriven,
    DateTimeOffset CreatedAt);
