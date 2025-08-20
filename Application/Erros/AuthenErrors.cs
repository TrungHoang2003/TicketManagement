using BuildingBlocks.Commons;

namespace Application.Erros;

public sealed record AuthenErrors
{
   public static readonly Error WrongPassWord = new Error("Authentication Error", "Wrong Password");
}