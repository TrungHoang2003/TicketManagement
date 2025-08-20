namespace BuildingBlocks.Commons;

public class ValidationException(Dictionary<string, string[]> errors) : Exception("Validation failed")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}

public class BusinessException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
