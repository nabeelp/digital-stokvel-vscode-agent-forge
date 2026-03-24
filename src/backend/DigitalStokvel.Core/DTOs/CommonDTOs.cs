namespace DigitalStokvel.Core.DTOs;

/// <summary>
/// Generic result wrapper for service operations
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public List<string> Errors { get; }

    private Result(bool isSuccess, T? data, string? errorMessage, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Errors = errors ?? new List<string>();
    }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
    public static Result<T> Failure(List<string> errors) => new(false, default, null, errors);
}

/// <summary>
/// Non-generic result for operations without return data
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public List<string> Errors { get; }

    private Result(bool isSuccess, string? errorMessage, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors ?? new List<string>();
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string errorMessage) => new(false, errorMessage);
    public static Result Failure(List<string> errors) => new(false, null, errors);
}

/// <summary>
/// Pagination request parameters
/// </summary>
public record PaginationRequest(
    int PageNumber = 1,
    int PageSize = 20
)
{
    public int Skip => (PageNumber - 1) * PageSize;
}

/// <summary>
/// Health check response
/// </summary>
public record HealthCheckResponse(
    string Status,
    DateTime Timestamp,
    string Version,
    Dictionary<string, string>? Dependencies = null
);
