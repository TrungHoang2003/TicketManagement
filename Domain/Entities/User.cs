using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User: IdentityUser<int>
{ 
    public string FullName { get; set; } 
    public int DepartmentId { get; set; } 
    public string? AvatarUrl { get; set; }
    
    public Department Department { get; set; }
    public List<TicketAssignee> AssignedTickets { get; set; }
    public List<Ticket> CreatedTickets { get; set; }
    public List<Ticket> FollowingTickets { get; set; }
}