
namespace Application.DTOs;

public class CreateDepartmentRequest
{
    public string Name { get; set; }
}

public class UpdateDepartmentRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    
}