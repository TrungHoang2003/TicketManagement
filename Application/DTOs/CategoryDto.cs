namespace Application.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; }
    public string Department { get; set; }
}

public class UpdateCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
}


