namespace Application.DTOs;

public class UpdateUserDto
{
    public int Id{ get; set; }
    public int? DepartmentId { get; set; } 
    public string? Username { get; set; }
    public string? FullName { get; set; } 
    public string? Role { get; set; }
}