using Application.Mappings;
using Application.Services;
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
   public string CreateAt { get; set; }
   public string Status { get; set; }
   
   public void Mapping(MappingProfile profile)
   {
       profile.CreateMap<Ticket, TicketDto>()
           .ForMember(dest => dest.AssigneeNames, opt =>
               opt.MapFrom(src => TicketService.GetNames(src.Assignees)));
   }
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

public class AssignTicketRequest
{
    public List<int> AssigneeIds { get; set; }
    public int TicketId { get; set; }
    public string Note { get; set; }
}

public class GetListTicketRequest
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public string? Status { get; set; }
    public string? Prority { get; set; }
    public int? CategoryId { get; set; }
    public string? Title { get; set; }
    public DateTime? CreateAt { get; set; }
}

public class GetListTicketResponse
{
    public List<TicketDto> Tickets { get; set; }
    public int TotalCount { get; set; }
}
