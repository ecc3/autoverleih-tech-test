namespace Api.DTOs;

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);

public record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);

public record CustomerResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateTimeOffset CreatedAt);
