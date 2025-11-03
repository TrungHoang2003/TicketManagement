using Application.Mappings;
using Domain.Entities;

namespace Application.DTOs;

public class ProgressDto: IMapFrom<Progress>
{
    public string TicketStatus { get; set; }
    public int TicketId { get; set; }
    public string Note { get; set;}
    public string EmployeeName { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int StepNumber { get; set; }
    public List<string> AttachmentUrls { get; set; }
}