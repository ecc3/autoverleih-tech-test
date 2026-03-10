using Api.Data;
using Api.DTOs;
using Api.Services;

namespace Api.Tests.Services;

public class CustomerServiceTests
{
    private readonly AppDbContext _context;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _context = TestHelper.CreateInMemoryContext();
        _service = new CustomerService(_context);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsSuccess()
    {
        var request = new CreateCustomerRequest("John", "Doe", "john@example.com", null);

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("John", result.Value!.FirstName);
        Assert.Equal("john@example.com", result.Value.Email);
    }

    [Fact]
    public async Task Create_DuplicateEmail_ReturnsConflict()
    {
        var request = new CreateCustomerRequest("John", "Doe", "john@example.com", null);
        await _service.CreateAsync(request);

        var result = await _service.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task GetAll_ReturnsAllCustomers()
    {
        await _service.CreateAsync(new CreateCustomerRequest("John", "Doe", "john@example.com", null));
        await _service.CreateAsync(new CreateCustomerRequest("Jane", "Doe", "jane@example.com", null));

        var result = await _service.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetById_ExistingCustomer_ReturnsSuccess()
    {
        var created = await _service.CreateAsync(new CreateCustomerRequest("John", "Doe", "john@example.com", null));

        var result = await _service.GetByIdAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("John", result.Value!.FirstName);
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
        var created = await _service.CreateAsync(new CreateCustomerRequest("John", "Doe", "john@example.com", null));

        var result = await _service.UpdateAsync(created.Value!.Id, new UpdateCustomerRequest("Johnny", "Doe", "john@example.com", "123456"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Johnny", result.Value!.FirstName);
        Assert.Equal("123456", result.Value.PhoneNumber);
    }

    [Fact]
    public async Task Update_DuplicateEmail_ReturnsConflict()
    {
        await _service.CreateAsync(new CreateCustomerRequest("John", "Doe", "john@example.com", null));
        var second = await _service.CreateAsync(new CreateCustomerRequest("Jane", "Doe", "jane@example.com", null));

        var result = await _service.UpdateAsync(second.Value!.Id, new UpdateCustomerRequest("Jane", "Doe", "john@example.com", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateCustomerRequest("John", "Doe", "john@example.com", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task Delete_ExistingCustomer_ReturnsSuccess()
    {
        var created = await _service.CreateAsync(new CreateCustomerRequest("John", "Doe", "john@example.com", null));

        var result = await _service.DeleteAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        var all = await _service.GetAllAsync();
        Assert.Empty(all.Value!);
    }

    [Fact]
    public async Task Delete_CustomerWithRentals_ReturnsConflict()
    {
        var created = await _service.CreateAsync(new CreateCustomerRequest("John", "Doe", "john@example.com", null));
        var car = new Api.Models.Car
        {
            Id = Guid.NewGuid(), Make = "BMW", Model = "3 Series",
            LicensePlate = "AB-123-CD", Year = 2024, CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Cars.Add(car);
        var rental = new Api.Models.Rental
        {
            Id = Guid.NewGuid(), CustomerId = created.Value!.Id, CarId = car.Id,
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
}
