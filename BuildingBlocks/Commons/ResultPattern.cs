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
    private static Result<T> Failure(Error error) => new(false, default!, error);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

public class Result<T,E>(bool success, T data, E secondData, Error? error) : Result(success, error)
{
    public T Data { get; } = data;
    public E SecondData { get; } = secondData;
    public static Result<T,E> IsSuccess(T value, E value2) => new(true, value, value2,null);
    private static Result<T,E> Failure(Error error) => new(false, default!, default!, error);
    public static implicit operator Result<T,E>(Error error) => Failure(error);
}

public class Error(string? code, string? description)
{
    public string? Code { get; } = code;
    public string? Description { get; } = description;
}

