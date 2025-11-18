using Application.Mappings;
using Domain.Entities;

namespace Application.DTOs;

public class UserDto: IMapFrom<User>
{
    public int Id { get; set; }
    public string FullName { get; set; } 
    public int DepartmentId { get; set; } 
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
}

public class CreateUserRequest
{
    public int DepartmentId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
}

public class UserLoginResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}

public class UsersByDepartmentDto
{
    public int Id { get; set; } 
    public string FullName { get; set; }
}

public class UpdateUserRequest
{
    public int Id{ get; set; }
    public int? DepartmentId { get; set; } 
    public string? Username { get; set; }
    public string? FullName { get; set; } 
    public List<string>? Roles { get; set; }
}

public class UserLoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
