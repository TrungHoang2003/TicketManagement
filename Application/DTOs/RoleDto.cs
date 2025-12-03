namespace Application.DTOs;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; }
    public string? ConcurrencyStamp { get; set; }
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateRoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
