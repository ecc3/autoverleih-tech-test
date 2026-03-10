using Api.Data;
using Api.DTOs;
using Api.Services;

namespace Api.Tests.Services;

public class CarServiceTests
{
    private readonly AppDbContext _context;
    private readonly CarService _service;

    public CarServiceTests()
    {
        _context = TestHelper.CreateInMemoryContext();
        _service = new CarService(_context);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsSuccess()
    {
        var request = new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024);

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("BMW", result.Value!.Make);
        Assert.Equal("AB-123-CD", result.Value.LicensePlate);
    }

    [Fact]
    public async Task Create_DuplicateLicensePlate_ReturnsConflict()
    {
        await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        var result = await _service.CreateAsync(new CreateCarRequest("Audi", "A4", "AB-123-CD", 2023));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task GetAll_ReturnsAllCars()
    {
        await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));
        await _service.CreateAsync(new CreateCarRequest("Audi", "A4", "EF-456-GH", 2023));

        var result = await _service.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetById_ExistingCar_ReturnsSuccess()
    {
        var created = await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        var result = await _service.GetByIdAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("BMW", result.Value!.Make);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsSuccess()
    {
        var created = await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        var result = await _service.UpdateAsync(created.Value!.Id, new UpdateCarRequest("BMW", "5 Series", "AB-123-CD", 2025));

        Assert.True(result.IsSuccess);
        Assert.Equal("5 Series", result.Value!.Model);
        Assert.Equal(2025, result.Value.Year);
    }

    [Fact]
    public async Task Update_DuplicateLicensePlate_ReturnsConflict()
    {
        await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));
        var second = await _service.CreateAsync(new CreateCarRequest("Audi", "A4", "EF-456-GH", 2023));

        var result = await _service.UpdateAsync(second.Value!.Id, new UpdateCarRequest("Audi", "A4", "AB-123-CD", 2023));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task Delete_ExistingCar_ReturnsSuccess()
    {
        var created = await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        var result = await _service.DeleteAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        var all = await _service.GetAllAsync();
        Assert.Empty(all.Value!);
    }

    [Fact]
    public async Task Delete_CarWithRentals_ReturnsConflict()
    {
        var created = await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));
        var customer = new Api.Models.Customer
        {
            Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe",
            Email = "john@example.com", CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Customers.Add(customer);
        var rental = new Api.Models.Rental
        {
            Id = Guid.NewGuid(), CustomerId = customer.Id, CarId = created.Value!.Id,
            StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(7),
            Status = Api.Models.RentalStatus.Active, CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(created.Value!.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task GetAll_IncludesTotalKilometers()
    {
        var car = await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        // Seed a completed rental with km
        var customer = new Api.Models.Customer
        {
            Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe",
            Email = "john@example.com", CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Customers.Add(customer);
        var rental = new Api.Models.Rental
        {
            Id = Guid.NewGuid(), CustomerId = customer.Id, CarId = car.Value!.Id,
            StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(7),
            Status = Api.Models.RentalStatus.Completed, KilometersDriven = 500,
            ReturnedAt = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        Assert.True(result.IsSuccess);
        var fetchedCar = result.Value!.First(c => c.Id == car.Value.Id);
        Assert.Equal(500, fetchedCar.TotalKilometers);
    }

    [Fact]
    public async Task GetById_IncludesTotalKilometers()
    {
        var car = await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        // Seed two completed rentals
        var customer = new Api.Models.Customer
        {
            Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe",
            Email = "john@example.com", CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Customers.Add(customer);
        _context.Rentals.AddRange(
            new Api.Models.Rental
            {
                Id = Guid.NewGuid(), CustomerId = customer.Id, CarId = car.Value!.Id,
                StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(7),
                Status = Api.Models.RentalStatus.Completed, KilometersDriven = 300,
                ReturnedAt = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow
            },
            new Api.Models.Rental
            {
                Id = Guid.NewGuid(), CustomerId = customer.Id, CarId = car.Value!.Id,
                StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(7),
                Status = Api.Models.RentalStatus.Completed, KilometersDriven = 200,
                ReturnedAt = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(car.Value.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(500, result.Value!.TotalKilometers);
    }

    [Fact]
    public async Task GetById_ExcludesActiveAndCancelledRentalsFromTotalKm()
    {
        var car = await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        var customer = new Api.Models.Customer
        {
            Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe",
            Email = "john@example.com", CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Customers.Add(customer);
        _context.Rentals.AddRange(
            new Api.Models.Rental
            {
                Id = Guid.NewGuid(), CustomerId = customer.Id, CarId = car.Value!.Id,
                StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(7),
                Status = Api.Models.RentalStatus.Completed, KilometersDriven = 300,
                ReturnedAt = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow
            },
            new Api.Models.Rental
            {
                Id = Guid.NewGuid(), CustomerId = customer.Id, CarId = car.Value!.Id,
                StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(7),
                Status = Api.Models.RentalStatus.Active, CreatedAt = DateTimeOffset.UtcNow
            },
            new Api.Models.Rental
            {
                Id = Guid.NewGuid(), CustomerId = customer.Id, CarId = car.Value!.Id,
                StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(7),
                Status = Api.Models.RentalStatus.Cancelled, CreatedAt = DateTimeOffset.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(car.Value.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(300, result.Value!.TotalKilometers);
    }

    [Fact]
    public async Task Create_ReturnsTotalKilometersZero()
    {
        var result = await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalKilometers);
    }

    [Fact]
    public async Task Update_ReturnsTotalKilometers()
    {
        var car = await _service.CreateAsync(new CreateCarRequest("BMW", "3 Series", "AB-123-CD", 2024));

        var customer = new Api.Models.Customer
        {
            Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe",
            Email = "john@example.com", CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Customers.Add(customer);
        _context.Rentals.Add(new Api.Models.Rental
        {
            Id = Guid.NewGuid(), CustomerId = customer.Id, CarId = car.Value!.Id,
            StartDate = DateTimeOffset.UtcNow, EndDate = DateTimeOffset.UtcNow.AddDays(7),
            Status = Api.Models.RentalStatus.Completed, KilometersDriven = 750,
            ReturnedAt = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.UpdateAsync(car.Value.Id, new UpdateCarRequest("BMW", "5 Series", "AB-123-CD", 2025));

        Assert.True(result.IsSuccess);
        Assert.Equal(750, result.Value!.TotalKilometers);
    }
}
