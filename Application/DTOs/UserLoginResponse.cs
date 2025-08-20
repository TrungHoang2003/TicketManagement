using Domain.Entities;

namespace Application.DTOs;

public class UserLoginResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}