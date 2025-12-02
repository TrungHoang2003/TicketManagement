using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Org.BouncyCastle.Asn1.Crmf;

namespace Application.DTOs;

public class TicketDto: IMapFrom<Ticket>
{
   public int Id { get; set; }
   public string Title { get; set; }
   public string Category { get; set; }
   public int CategoryId { get; set; }
   public string? HeadDepartment { get; set; }
   public string Creator { get; set; }
   public string Priority { get; set; }
   public string? AssigneeNames { get; set; }
   public string CreateAt { get; set; }
   public string Status { get; set; }
   
   public void Mapping(MappingProfile profile)
   {
       profile.CreateMap<Ticket, TicketDto>()
           .ForMember(dest => dest.AssigneeNames, opt =>
               opt.MapFrom(src => TicketService.GetAssigneeNames(src.Assignees)))
           .ForMember(dest => dest.Category, opt =>
               opt.MapFrom(src => src.Category.Name))
           .ForMember(dest => dest.HeadDepartment, opt =>
               opt.MapFrom(src => TicketService.GetHeadNames(src.Heads)))
           .ForMember(dest => dest.Creator, opt =>
           opt.MapFrom(src => src.Creator.FullName));
   }
}

public class TicketDetailDto : IMapFrom<Ticket>
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string CauseType { get; set; }
    public string? Cause { get; set; }
    public string ImplementationPlan { get; set; }
    public string? AssigneeNames { get; set; }
    public string Category { get; set; }
    public string? HeadDepartment { get; set; }
    public string Content { get; set; }
    public DateTime DesiredCompleteDate { get; set; }
    public DateTime? ExpectedStartDate { get; set; }
    public DateTime? ExpectedCompleteDate { get; set; }
    public List<string> FileUrls { get; set; } = [];
    public List<string> FileNames { get; set; } = [];
    public string Status { get; set; }
    public string Priority { get; set; }
    public string Creator { get; set; }
    public string CreateAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? CompletionNote { get; set; }
    public List<ProgressDto> Progresses { get; set; } = [];
    public List<HeadDto> Heads { get; set; } = [];
    public List<UserDto> Assignees { get; set; } = [];

    public void Mapping(MappingProfile profile)
    {
        profile.CreateMap<Ticket, TicketDetailDto>()
            .ForMember(dest => dest.AssigneeNames, opt =>
                opt.MapFrom(src => TicketService.GetAssigneeNames(src.Assignees)))
            .ForMember(dest => dest.Assignees, opt =>
                opt.MapFrom(src => src.Assignees.Select(ta => ta.Assignee).ToList()))
            .ForMember(dest => dest.Category, opt =>
                opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.HeadDepartment, opt =>
                opt.MapFrom(src => TicketService.GetHeadNames(src.Heads)))
            .ForMember(dest => dest.CauseType, opt =>
                opt.MapFrom(src => src.CauseType != null ? src.CauseType.Name : null))
            .ForMember(dest => dest.Creator, opt =>
                opt.MapFrom(src => src.Creator.FullName))
            .ForMember(dest => dest.CreateAt, opt =>
                opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")));
    }
}

public class CreateTicketRequest
{
    public string Title { get; set;  }
    public int CategoryId { get; set; } 
    public string Content { get; set; }
    public string Priority { get; set; }
    public DateTime DesiredCompleteDate{ get; set; }
    public List<string>? Base64Files { get; set; }
    public List<string>? FileNames { get; set; }
}

public class UpdateTicketRequest
{
    public int TicketId { get; set; }
    public List<string>? FileNames { get; set; }
    public int? CauseTypeId { get; set; } 
    public string? ImplementationPlan { get; set; }
    public string? Cause { get; set; }
    public DateTime? ExpectedStartDate { get; set; }
    public DateTime? ExpectedCompleteDate{ get; set; }
}

public class ForwardTicketRequest
{
    public int TicketId { get; set; }
    public int HeadId{ get; set; }
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

public class AddHeadRequest
{
    public List<int> HeadIds { get; set; }
    public int TicketId { get; set; }
    public string Note { get; set; }
}

public class CompleteTicketDto
{
    public int TicketId { get; set; }
    public string CompletionNote { get; set; }
}

public class GetListTicketRequest
{
    public bool? IsAssigned{ get; set; }
    public bool? IsCreated { get; set; }
    public bool? IsFollowing { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public string? Status { get; set; }
    public string? Prority { get; set; }
    public int? CategoryId { get; set; }
    public string? Title { get; set; }
    public DateTime? CreateAt { get; set; }
}

public class HeadDto : IMapFrom<Domain.Entities.TicketHead>
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public bool IsMainHead { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Domain.Entities.TicketHead, HeadDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Head.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Head.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Head.Email))
            .ForMember(dest => dest.IsMainHead, opt => opt.MapFrom(src => src.IsMainHead));
    }
}

public class GetListTicketResponse
{
    public List<TicketDto> Tickets { get; set; }
    public int TotalCount { get; set; }
}

public class UnassignEmployeeRequest
{
    public int TicketId { get; set; }
    public int EmployeeId { get; set; }
}

public class DashboardTicketDto
{
    public int TotalTicket { get; set; } 
    public int TotalCreatedTicket { get; set; }
    public int TotalAssignedTicket { get; set; }
    public int TotalFollowingTicket { get; set; }
    public int TotalUnReceivedTicket { get; set; }
    public int TotalInProgressTicket { get; set; }
    public int TotalCompletedTicket { get; set; }
    public int TotalRejectedTicket { get; set; }
    public int TotalClosedTicket { get; set; }
    public List<CauseTypeStatDto>? TicketsByCauseType { get; set; }
    public List<CategoryStatDto>? TicketsByCategory { get; set; }
    public List<PriorityStatDto>? TicketsByPriority { get; set; }
    public List<StatusTimelineDto>? TicketsByMonth { get; set; }
    public PerformanceStatDto? Performance { get; set; }
}

public class CauseTypeStatDto
{
    public string CauseTypeName { get; set; }
    public int Count { get; set; }
}

public class CategoryStatDto
{
    public string CategoryName { get; set; }
    public int Count { get; set; }
}

public class PriorityStatDto
{
    public string Priority { get; set; }
    public int Count { get; set; }
}

public class StatusTimelineDto
{
    public string Month { get; set; }
    public int Created { get; set; }
    public int Completed { get; set; }
    public int InProgress { get; set; }
}

public class PerformanceStatDto
{
    public double AverageCompletionDays { get; set; }
    public int OnTimeTickets { get; set; }
    public int OverdueTickets { get; set; }
    public double CompletionRate { get; set; }
}
