using BuildingBlocks.Commons;

namespace Application.Erros;

public sealed record GoogleErrors 
{
    public static readonly Error AuthFailed= new ("Google Auth Error", "Failed to retrieve token"); 
    public static readonly Error InvalidToken= new ("Google Invalid Token", "Invalid Id token"); 
    public static readonly Error AuthException= new ("Google Auth Exception", "Error validating Google code"); 
}