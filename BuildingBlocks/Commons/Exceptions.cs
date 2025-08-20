namespace BuildingBlocks.Commons;

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(Dictionary<string, string[]> errors) : base("Validation failed")
    {
        Errors = errors;
    }
}

public class BusinessException : Exception
{
    public string ErrorCode { get; }
    
    public BusinessException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
