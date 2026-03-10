using Api.Services;

namespace Api.Endpoints;

public static class EndpointHelper
{
    public static IResult MapError<T>(Result<T> result) => result.ErrorType switch
    {
        ResultErrorType.NotFound => Results.NotFound(new { error = result.ErrorMessage }),
        ResultErrorType.Conflict => Results.Conflict(new { error = result.ErrorMessage }),
        ResultErrorType.ValidationError => Results.BadRequest(new { error = result.ErrorMessage }),
        _ => Results.Problem("An unexpected error occurred.")
    };
}
