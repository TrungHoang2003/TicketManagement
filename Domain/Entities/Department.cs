using Domain.Interfaces;

namespace Domain.Entities;

public class Department: Entity
{
   public string Name { get; set; }
   
   public List<User> Employees { get; set; }
}