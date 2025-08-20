using BuildingBlocks.Commons;

namespace Application.Erros;

public sealed record UserErrors
{
   public static readonly Error UserNotFound = new Error("User Error", "User Not Found");
}