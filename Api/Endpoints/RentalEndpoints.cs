using Api.DTOs;
using Api.Services;

namespace Api.Endpoints;

public static class RentalEndpoints
{
    public static void MapRentalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rentals").WithTags("Rentals");

        group.MapPost("/", async (CreateRentalRequest request, RentalService service) =>
        {
            var result = await service.CreateAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/rentals/{result.Value!.Id}", result.Value)
                : EndpointHelper.MapError(result);
        });

        group.MapGet("/", async (RentalService service) =>
        {
            var result = await service.GetAllAsync();
            return Results.Ok(result.Value);
        });

        group.MapGet("/{id:guid}", async (Guid id, RentalService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointHelper.MapError(result);
        });

        group.MapPost("/{id:guid}/return", async (Guid id, ReturnRentalRequest request, RentalService service) =>
        {
            var result = await service.ReturnAsync(id, request.KilometersDriven);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointHelper.MapError(result);
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, RentalService service) =>
        {
            var result = await service.CancelAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointHelper.MapError(result);
        });
    }

}
