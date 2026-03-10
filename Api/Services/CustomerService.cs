using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class CustomerService(AppDbContext context)
{
    public async Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request)
    {
        if (await context.Customers.AnyAsync(c => c.Email == request.Email))
            return Result<CustomerResponse>.Failure("A customer with this email already exists.", ResultErrorType.Conflict);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return Result<CustomerResponse>.Success(MapToResponse(customer));
    }

    public async Task<Result<List<CustomerResponse>>> GetAllAsync()
    {
        var customers = await context.Customers.ToListAsync();
        return Result<List<CustomerResponse>>.Success(customers.Select(MapToResponse).ToList());
    }

    public async Task<Result<CustomerResponse>> GetByIdAsync(Guid id)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer is null)
            return Result<CustomerResponse>.Failure("Customer not found.", ResultErrorType.NotFound);

        return Result<CustomerResponse>.Success(MapToResponse(customer));
    }

    public async Task<Result<CustomerResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer is null)
            return Result<CustomerResponse>.Failure("Customer not found.", ResultErrorType.NotFound);

        if (await context.Customers.AnyAsync(c => c.Email == request.Email && c.Id != id))
            return Result<CustomerResponse>.Failure("A customer with this email already exists.", ResultErrorType.Conflict);

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.PhoneNumber = request.PhoneNumber;

        await context.SaveChangesAsync();

        return Result<CustomerResponse>.Success(MapToResponse(customer));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var customer = await context.Customers
            .Include(c => c.Rentals)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
            return Result<bool>.Failure("Customer not found.", ResultErrorType.NotFound);

        if (customer.Rentals.Count != 0)
            return Result<bool>.Failure("Cannot delete customer with associated rentals.", ResultErrorType.Conflict);

        context.Customers.Remove(customer);
        await context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    internal static CustomerResponse MapToResponse(Customer customer) =>
        new(customer.Id, customer.FirstName, customer.LastName,
            customer.Email, customer.PhoneNumber, customer.CreatedAt);
}
