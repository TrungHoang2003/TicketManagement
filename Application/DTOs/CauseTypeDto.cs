namespace Application.DTOs;

public class CreateCauseTypeDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class UpdateCauseTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class CauseTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}