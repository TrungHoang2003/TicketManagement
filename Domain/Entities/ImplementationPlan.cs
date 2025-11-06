using Domain.Interfaces;

namespace Domain.Entities;

public class ImplementationPlan : Entity
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public List<Ticket> Tickets { get; set; }
}