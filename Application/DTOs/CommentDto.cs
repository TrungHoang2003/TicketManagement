using Application.Mappings;
using Domain.Entities;

namespace Application.DTOs;

public class CommentDto: IMapFrom<Comment>
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatorName { get; set; }
    public string CreatorEmail { get; set; }
    public List<string> AttachmentUrls { get; set; }
}

public class CreateCommentRequest
{
    public int TicketId { get; set; }
    public string Content { get; set; }
    public List<string>? Base64Files{ get; set; }
}