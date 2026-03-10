using Api.Data;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

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

        return Result<CarResponse>.Success(MapToResponse(car, 0));
    }

    public async Task<Result<List<CarResponse>>> GetAllAsync()
    {
        var cars = await context.Cars.ToListAsync();
        var carIds = cars.Select(c => c.Id).ToList();

        var kmBycar = await context.Rentals
            .Where(r => carIds.Contains(r.CarId) && r.Status == RentalStatus.Completed && r.KilometersDriven != null)
            .GroupBy(r => r.CarId)
            .Select(g => new { CarId = g.Key, TotalKm = g.Sum(r => r.KilometersDriven!.Value) })
            .ToDictionaryAsync(x => x.CarId, x => x.TotalKm);

        return Result<List<CarResponse>>.Success(
            cars.Select(c => MapToResponse(c, kmBycar.GetValueOrDefault(c.Id, 0))).ToList());
    }

    public async Task<Result<CarResponse>> GetByIdAsync(Guid id)
    {
        var car = await context.Cars.FindAsync(id);
        if (car is null)
            return Result<CarResponse>.Failure("Car not found.", ResultErrorType.NotFound);

        var totalKm = await context.Rentals
            .Where(r => r.CarId == id && r.Status == RentalStatus.Completed && r.KilometersDriven != null)
            .SumAsync(r => r.KilometersDriven!.Value);

        return Result<CarResponse>.Success(MapToResponse(car, totalKm));
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

        var totalKm = await context.Rentals
            .Where(r => r.CarId == id && r.Status == RentalStatus.Completed && r.KilometersDriven != null)
            .SumAsync(r => r.KilometersDriven!.Value);

        return Result<CarResponse>.Success(MapToResponse(car, totalKm));
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

    internal static CarResponse MapToResponse(Car car, int totalKilometers) =>
        new(car.Id, car.Make, car.Model, car.LicensePlate,
            car.Year, car.IsAvailable, totalKilometers, car.CreatedAt);
}
