using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class RentalService(AppDbContext context)
{
    public async Task<Result<RentalResponse>> CreateAsync(CreateRentalRequest request)
    {
        var customer = await context.Customers.FindAsync(request.CustomerId);
        if (customer is null)
            return Result<RentalResponse>.Failure("Customer not found.", ResultErrorType.NotFound);

        var car = await context.Cars.FindAsync(request.CarId);
        if (car is null)
            return Result<RentalResponse>.Failure("Car not found.", ResultErrorType.NotFound);

        if (!car.IsAvailable)
            return Result<RentalResponse>.Failure("Car is not available.", ResultErrorType.Conflict);

        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            CarId = request.CarId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = RentalStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        car.IsAvailable = false;

        context.Rentals.Add(rental);
        await context.SaveChangesAsync();

        return Result<RentalResponse>.Success(MapToResponse(rental));
    }

    public async Task<Result<List<RentalResponse>>> GetAllAsync()
    {
        var rentals = await context.Rentals.ToListAsync();
        return Result<List<RentalResponse>>.Success(rentals.Select(MapToResponse).ToList());
    }

    public async Task<Result<RentalResponse>> GetByIdAsync(Guid id)
    {
        var rental = await context.Rentals.FindAsync(id);
        if (rental is null)
            return Result<RentalResponse>.Failure("Rental not found.", ResultErrorType.NotFound);

        return Result<RentalResponse>.Success(MapToResponse(rental));
    }

    public async Task<Result<RentalResponse>> ReturnAsync(Guid id)
    {
        var rental = await context.Rentals.FindAsync(id);
        if (rental is null)
            return Result<RentalResponse>.Failure("Rental not found.", ResultErrorType.NotFound);

        if (rental.Status != RentalStatus.Active)
            return Result<RentalResponse>.Failure("Only active rentals can be returned.", ResultErrorType.Conflict);

        var car = await context.Cars.FindAsync(rental.CarId);
        if (car is null)
            return Result<RentalResponse>.Failure("Car not found.", ResultErrorType.NotFound);

        rental.Status = RentalStatus.Completed;
        rental.ReturnedAt = DateTimeOffset.UtcNow;
        car.IsAvailable = true;

        await context.SaveChangesAsync();

        return Result<RentalResponse>.Success(MapToResponse(rental));
    }

    public async Task<Result<RentalResponse>> CancelAsync(Guid id)
    {
        var rental = await context.Rentals.FindAsync(id);
        if (rental is null)
            return Result<RentalResponse>.Failure("Rental not found.", ResultErrorType.NotFound);

        if (rental.Status != RentalStatus.Active)
            return Result<RentalResponse>.Failure("Only active rentals can be cancelled.", ResultErrorType.Conflict);

        var car = await context.Cars.FindAsync(rental.CarId);
        if (car is null)
            return Result<RentalResponse>.Failure("Car not found.", ResultErrorType.NotFound);

        rental.Status = RentalStatus.Cancelled;
        car.IsAvailable = true;

        await context.SaveChangesAsync();

        return Result<RentalResponse>.Success(MapToResponse(rental));
    }

    internal static RentalResponse MapToResponse(Rental rental) =>
        new(rental.Id, rental.CustomerId, rental.CarId,
            rental.StartDate, rental.EndDate, rental.ReturnedAt,
            rental.Status.ToString(), rental.CreatedAt);
}
