namespace BuildingBlocks.Commons;

public abstract class ValidationException(Dictionary<string, string[]> errors) : Exception("Validation failed")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}

public class BusinessException(string message) : Exception(message)
{
    public string ErrorCode => "Business Error";
}
