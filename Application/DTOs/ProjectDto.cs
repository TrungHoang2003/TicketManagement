namespace Application.DTOs;

public class CreateProjectRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class UpdateProjectRequest
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