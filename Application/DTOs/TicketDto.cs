using Application.Mappings;
using Domain.Entities;

namespace Application.DTOs;

public class TicketDto: IMapFrom<Ticket>
{
   public int Id { get; set; }
   public string Title { get; set; }
   public int CategoryId { get; set; }
   public int? HeadDepartmentId { get; set; }
   public int CreatorId { get; set; }
   public string Priority { get; set; }
   public string AssigneeNames { get; set; }
   public string CreateDate { get; set; }
   public string Status { get; set; }
}

public class CreateTicketRequest
{
    public string Title { get; set;  }
    public int CategoryId { get; set; } 
    public string Content { get; set; }
    public string Priority { get; set; }
    public int ProjectId { get; set; }
    public DateTime DesiredCompleteDate{ get; set; }
    public List<string>? Base64Files { get; set; }
}

public class RejectTicketDto
{
    public int TicketId { get; set; } 
    public string Reason { get; set; }
}

public class AssignTicketDto
{
    public int AssigneeId { get; set; }
    public int TicketId { get; set; }
    public string Note { get; set; }
}

public class GetListTicketRequest
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}

public class GetListTicketResponse
{
    public List<TicketDto> Tickets { get; set; }
    public int TotalCount { get; set; }
}
