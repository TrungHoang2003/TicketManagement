using Domain.Entities;

namespace Application.DTOs;

public class CreateTicketDto
{
    public string Title { get; set;  }
    public int CategoryId { get; set; } 
    public string Content { get; set; }
    public string Priority { get; set; }
    public List<string>? Base64Files { get; set; }
}