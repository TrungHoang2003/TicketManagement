namespace Application.DTOs;

public class CreateProjectDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class UpdateProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}