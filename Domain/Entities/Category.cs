using Domain.Interfaces;

namespace Domain.Entities;

public class Category: Entity
{
    public string Name { get; set; } 
    public DepartmentEnum Department { get; set; }
}

public enum DepartmentEnum
{
    Ad = 1,
    It = 2,
    Qa = 3
}