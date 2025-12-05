using Domain.Interfaces;

namespace Domain.Entities;

public class Ticket: Entity
{
    public string Title { get; set;  }
    public int CategoryId { get; set; } 
    public int CreatorId { get; set; }
    public int? CauseTypeId { get; set; }
    public string? ImplementationPlan { get; set; }
    public string? Cause { get; set; }
    public DateTime DesiredCompleteDate{ get; set; }
    public DateTime? ExpectedStartDate { get; set; }
    public DateTime? ExpectedCompleteDate{ get; set; }
    public string Content { get; set; }
    public Priority Priority { get; set; }
    public Status Status { get; set; } = Status.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    
    // Navigation properties
    public List<Comment> Comments { get; set; } = [];
    public Category Category{ get; set; }
    public User Creator { get; set; }
    public CauseType? CauseType { get; set; }
    public List<Progress> Progresses { get; set; } = [];
    public List<TicketAssignee> Assignees { get; set; } = [];
    public List<TicketHead> Heads{ get; set; } = [];
    public List<History> Histories { get; set; } = [];
}

public enum Priority
{
    Critical = 0,
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
    Completed = 5,
    Closed = 6
}