namespace Api.Services;

public enum ResultErrorType
{
    ValidationError,
    NotFound,
    Conflict
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public ResultErrorType? ErrorType { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string errorMessage, ResultErrorType errorType)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string errorMessage, ResultErrorType errorType) => new(errorMessage, errorType);
}
