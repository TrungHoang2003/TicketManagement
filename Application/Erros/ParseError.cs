using BuildingBlocks.Commons;

namespace Application.Erros;

public sealed record ParseError
{
    public static readonly Error UserIdParseError= new("Parse Error", "Cannot parse userIdClaim into int value");
}