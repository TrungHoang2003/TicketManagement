using Application.Mappings;
using Application.Services;
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
    public string CauseType { get; set; }
    public string ImplementationPlan { get; set; }
    public string? AssigneeNames { get; set; }
    public string Category { get; set; }
    public string? HeadDepartment { get; set; }
    public string Content { get; set; }
    public DateTime DesiredCompleteDate { get; set; }
    public DateTime? ExpectedStartDate { get; set; }
    public DateTime? ExpectedCompleteDate { get; set; }
    public List<string> FileUrls { get; set; } = [];

    public void Mapping(MappingProfile profile)
    {
        profile.CreateMap<Ticket, TicketDetailDto>()
            .ForMember(dest => dest.AssigneeNames, opt =>
                opt.MapFrom(src => TicketService.GetAssigneeNames(src.Assignees)))
            .ForMember(dest => dest.Category, opt =>
                opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.HeadDepartment, opt =>
                opt.MapFrom(src => TicketService.GetHeadNames(src.Heads)))
            .ForMember(dest => dest.CauseType, opt =>
                opt.MapFrom(src => src.CauseType != null ? src.CauseType.Name : null));
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

public class GetListTicketResponse
{
    public List<TicketDto> Tickets { get; set; }
    public int TotalCount { get; set; }
}
