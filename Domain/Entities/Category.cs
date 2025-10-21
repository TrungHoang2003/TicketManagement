using Domain.Interfaces;

namespace Domain.Entities;

public class Category: Entity
{
    public string Name { get; set; } 
    public int DepartmentId { get; set; }
}
