namespace API.DTOs;

public record CreateRentalRequest(
    Guid CustomerId,
    Guid CarId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);

public record RentalResponse(
    Guid Id,
    Guid CustomerId,
    Guid CarId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    DateTimeOffset? ReturnedAt,
    string Status,
    DateTimeOffset CreatedAt);
