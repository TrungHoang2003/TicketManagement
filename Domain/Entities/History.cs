using Domain.Interfaces;

namespace Domain.Entities;

public class History: Entity
{
   public int TicketId { get; set; }
   public Ticket Ticket { get; set; } 
}