using Domain.Interfaces;

namespace Domain.Entities;

public class History: Entity
{
   public string Content { get; set; }
   public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
   public int TicketId { get; set; }
   public Ticket Ticket { get; set; } 

}