using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class CarService(AppDbContext context)
{
    public async Task<Result<CarResponse>> CreateAsync(CreateCarRequest request)
    {
        if (await context.Cars.AnyAsync(c => c.LicensePlate == request.LicensePlate))
            return Result<CarResponse>.Failure("A car with this license plate already exists.", ResultErrorType.Conflict);

        var car = new Car
        {
            Id = Guid.NewGuid(),
            Make = request.Make,
            Model = request.Model,
            LicensePlate = request.LicensePlate,
            Year = request.Year,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Cars.Add(car);
        await context.SaveChangesAsync();

        return Result<CarResponse>.Success(MapToResponse(car));
    }

    public async Task<Result<List<CarResponse>>> GetAllAsync()
    {
        var cars = await context.Cars.ToListAsync();
        return Result<List<CarResponse>>.Success(cars.Select(MapToResponse).ToList());
    }

    public async Task<Result<CarResponse>> GetByIdAsync(Guid id)
    {
        var car = await context.Cars.FindAsync(id);
        if (car is null)
            return Result<CarResponse>.Failure("Car not found.", ResultErrorType.NotFound);

        return Result<CarResponse>.Success(MapToResponse(car));
    }

    public async Task<Result<CarResponse>> UpdateAsync(Guid id, UpdateCarRequest request)
    {
        var car = await context.Cars.FindAsync(id);
        if (car is null)
            return Result<CarResponse>.Failure("Car not found.", ResultErrorType.NotFound);

        if (await context.Cars.AnyAsync(c => c.LicensePlate == request.LicensePlate && c.Id != id))
            return Result<CarResponse>.Failure("A car with this license plate already exists.", ResultErrorType.Conflict);

        car.Make = request.Make;
        car.Model = request.Model;
        car.LicensePlate = request.LicensePlate;
        car.Year = request.Year;

        await context.SaveChangesAsync();

        return Result<CarResponse>.Success(MapToResponse(car));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var car = await context.Cars
            .Include(c => c.Rentals)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car is null)
            return Result<bool>.Failure("Car not found.", ResultErrorType.NotFound);

        if (car.Rentals.Count != 0)
            return Result<bool>.Failure("Cannot delete car with associated rentals.", ResultErrorType.Conflict);

        context.Cars.Remove(car);
        await context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    internal static CarResponse MapToResponse(Car car) =>
        new(car.Id, car.Make, car.Model, car.LicensePlate,
            car.Year, car.IsAvailable, car.CreatedAt);
}
