using API.DTOs;
using API.Services;

namespace API.Endpoints;

public static class CarEndpoints
{
    public static void MapCarEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cars").WithTags("Cars");

        group.MapPost("/", async (CreateCarRequest request, CarService service) =>
        {
            var result = await service.CreateAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/cars/{result.Value!.Id}", result.Value)
                : EndpointHelper.MapError(result);
        });

        group.MapGet("/", async (CarService service) =>
        {
            var result = await service.GetAllAsync();
            return Results.Ok(result.Value);
        });

        group.MapGet("/{id:guid}", async (Guid id, CarService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointHelper.MapError(result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCarRequest request, CarService service) =>
        {
            var result = await service.UpdateAsync(id, request);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointHelper.MapError(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, CarService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.IsSuccess ? Results.NoContent() : EndpointHelper.MapError(result);
        });
    }

}
