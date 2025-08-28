using BuildingBlocks.Commons;

namespace Application.Erros;

public sealed record AuthenErrors
{
   public static readonly Error WrongPassWord = new Error("Authentication Error", "Wrong Password");
   public static readonly Error NotAuthorized = new Error("Authentication Error", "You dont have permission for this action");
}