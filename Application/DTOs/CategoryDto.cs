namespace Application.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; }
    public int DepartmentId { get; set; }
}

public class UpdateCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
}


