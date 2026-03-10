using Api.Data;
using Api.DTOs;
using Api.Models;
using Api.Services;

namespace Api.Tests.Services;

public class RentalServiceTests
{
    private readonly AppDbContext _context;
    private readonly RentalService _service;

    public RentalServiceTests()
    {
        _context = TestHelper.CreateInMemoryContext();
        _service = new RentalService(_context);
    }

    private async Task<Customer> SeedCustomer(string email = "john@example.com")
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    private async Task<Car> SeedCar(string plate = "AB-123-CD")
    {
        var car = new Car
        {
            Id = Guid.NewGuid(),
            Make = "BMW",
            Model = "3 Series",
            LicensePlate = plate,
            Year = 2024,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return car;
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsSuccess()
    {
        var customer = await SeedCustomer();
        var car = await SeedCar();
        var request = new CreateRentalRequest(customer.Id, car.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Active", result.Value!.Status);

        var updatedCar = await _context.Cars.FindAsync(car.Id);
        Assert.False(updatedCar!.IsAvailable);
    }

    [Fact]
    public async Task Create_NonExistentCustomer_ReturnsNotFound()
    {
        var car = await SeedCar();
        var request = new CreateRentalRequest(Guid.NewGuid(), car.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));

        var result = await _service.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task Create_NonExistentCar_ReturnsNotFound()
    {
        var customer = await SeedCustomer();
        var request = new CreateRentalRequest(customer.Id, Guid.NewGuid(),
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));

        var result = await _service.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task Create_UnavailableCar_ReturnsConflict()
    {
        var customer = await SeedCustomer();
        var car = await SeedCar();
        car.IsAvailable = false;
        await _context.SaveChangesAsync();

        var request = new CreateRentalRequest(customer.Id, car.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));

        var result = await _service.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task GetAll_ReturnsAllRentals()
    {
        var customer = await SeedCustomer();
        var car1 = await SeedCar("AB-123-CD");
        var car2 = await SeedCar("EF-456-GH");
        await _service.CreateAsync(new CreateRentalRequest(customer.Id, car1.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)));
        await _service.CreateAsync(new CreateRentalRequest(customer.Id, car2.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)));

        var result = await _service.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetById_ExistingRental_ReturnsSuccess()
    {
        var customer = await SeedCustomer();
        var car = await SeedCar();
        var created = await _service.CreateAsync(new CreateRentalRequest(customer.Id, car.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)));

        var result = await _service.GetByIdAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Active", result.Value!.Status);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task Return_ActiveRental_ReturnsSuccess()
    {
        var customer = await SeedCustomer();
        var car = await SeedCar();
        var created = await _service.CreateAsync(new CreateRentalRequest(customer.Id, car.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)));

        var result = await _service.ReturnAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Completed", result.Value!.Status);
        Assert.NotNull(result.Value.ReturnedAt);

        var updatedCar = await _context.Cars.FindAsync(car.Id);
        Assert.True(updatedCar!.IsAvailable);
    }

    [Fact]
    public async Task Return_NonActiveRental_ReturnsConflict()
    {
        var customer = await SeedCustomer();
        var car = await SeedCar();
        var created = await _service.CreateAsync(new CreateRentalRequest(customer.Id, car.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)));
        await _service.ReturnAsync(created.Value!.Id);

        var result = await _service.ReturnAsync(created.Value!.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task Return_NonExistent_ReturnsNotFound()
    {
        var result = await _service.ReturnAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task Cancel_ActiveRental_ReturnsSuccess()
    {
        var customer = await SeedCustomer();
        var car = await SeedCar();
        var created = await _service.CreateAsync(new CreateRentalRequest(customer.Id, car.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)));

        var result = await _service.CancelAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Cancelled", result.Value!.Status);
        Assert.Null(result.Value.ReturnedAt);

        var updatedCar = await _context.Cars.FindAsync(car.Id);
        Assert.True(updatedCar!.IsAvailable);
    }

    [Fact]
    public async Task Cancel_NonActiveRental_ReturnsConflict()
    {
        var customer = await SeedCustomer();
        var car = await SeedCar();
        var created = await _service.CreateAsync(new CreateRentalRequest(customer.Id, car.Id,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7)));
        await _service.CancelAsync(created.Value!.Id);

        var result = await _service.CancelAsync(created.Value!.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task Cancel_NonExistent_ReturnsNotFound()
    {
        var result = await _service.CancelAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }
}
