namespace Application.DTOs;

public class CreateUserDto
{
    public int DepartmentId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}