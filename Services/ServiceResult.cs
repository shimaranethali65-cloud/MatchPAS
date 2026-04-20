namespace BlindMatchPAS.Services;

public sealed class ServiceResult
{
    public bool Success { get; private init; }
    public string? Error { get; private init; }

    public static ServiceResult Ok() => new() { Success = true };

    public static ServiceResult Fail(string error) => new() { Success = false, Error = error };
}

public sealed class ServiceResult<T>
{
    public bool Success { get; private init; }
    public string? Error { get; private init; }
    public T? Data { get; private init; }

    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };

    public static ServiceResult<T> Fail(string error) => new() { Success = false, Error = error };
}
