namespace BuildingBlocks.Commons;

public class Result(bool success, Error? error)
{
    public bool Success { get; } = success;
    public Error? Error { get; } = error;
    public static Result IsSuccess() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<T>(bool success, T data, Error? error) : Result(success, error)
{
    public T Data { get; } = data;
    public static Result<T> IsSuccess(T value) => new(true, value, null);
    private new static Result<T> Failure(Error error) => new(false, default!, error);
    public static implicit operator Result<T>(Error error) => Failure(error);
    public static implicit operator Result<T>(T data) => IsSuccess(data);
}

public class Result<T,TE>(bool success, T data, TE secondData, Error? error) : Result(success, error)
{
    public T Data { get; } = data;
    public TE SecondData { get; } = secondData;
    public static Result<T,TE> IsSuccess(T value, TE value2) => new(true, value, value2,null);
    private new static Result<T,TE> Failure(Error error) => new(false, default!, default!, error);
    public static implicit operator Result<T,TE>(Error error) => Failure(error);
}

public class Error(string? code, string? description)
{
    public string? Code { get; } = code;
    public string? Description { get; } = description;
}

