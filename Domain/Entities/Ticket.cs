using Domain.Interfaces;

namespace Domain.Entities;

public class Ticket: Entity
{
    public string Title { get; set;  }
    public int CategoryId { get; set; } 
    public int CreatorId { get; set; }
    public int? AssigneeId{ get; set; }
    public int HeadDepartmentId { get; set; }
    public string Content { get; set; }
    public Priority Priority { get; set; }
    public Status Status { get; set; } = Status.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    
    // Navigation properties
    public List<Comment> Comments { get; set; } = [];
    public Category Category{ get; set; }
    public User Creator { get; set; }
    public User? Assignee { get; set; }
    public User HeadOfDepartment { get; set; }
    public List<Progress> Progresses { get; set; } = [];
}

public enum Priority
{
    High = 1,
    Medium = 2,
    Low = 3
}

public enum Status
{
    Pending = 1,
    Received = 2,
    InProgress = 3,
    Rejected = 4,
    Closed = 5
}