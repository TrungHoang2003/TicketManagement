namespace Application.DTOs;

public class CreateImplementationPlanRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class UpdateImplementationPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class ImplementationPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}