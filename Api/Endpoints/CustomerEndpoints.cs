using Api.DTOs;
using Api.Services;

namespace Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers");

        group.MapPost("/", async (CreateCustomerRequest request, CustomerService service) =>
        {
            var result = await service.CreateAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/customers/{result.Value!.Id}", result.Value)
                : EndpointHelper.MapError(result);
        });

        group.MapGet("/", async (CustomerService service) =>
        {
            var result = await service.GetAllAsync();
            return Results.Ok(result.Value);
        });

        group.MapGet("/{id:guid}", async (Guid id, CustomerService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointHelper.MapError(result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCustomerRequest request, CustomerService service) =>
        {
            var result = await service.UpdateAsync(id, request);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointHelper.MapError(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, CustomerService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.IsSuccess ? Results.NoContent() : EndpointHelper.MapError(result);
        });
    }

}
