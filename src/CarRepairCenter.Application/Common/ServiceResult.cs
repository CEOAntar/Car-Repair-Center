namespace CarRepairCenter.Application.Common;

/// <summary>
/// Lightweight result wrapper for service operations.
/// Allows services to communicate success/failure without throwing exceptions
/// or depending on HTTP-specific types.
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public bool IsNotFound { get; }

    private ServiceResult(bool success, T? data, string? error, bool notFound = false)
    {
        IsSuccess = success;
        Data = data;
        ErrorMessage = error;
        IsNotFound = notFound;
    }

    public static ServiceResult<T> Success(T data) => new(true, data, null);
    public static ServiceResult<T> Failure(string error) => new(false, default, error);
    public static ServiceResult<T> NotFound(string? error = null) => new(false, default, error, notFound: true);
}
