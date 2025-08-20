namespace Domain.Entities;

public class Attachment
{
   public int Id { get; set; } 
   public int EntityId { get; set; }
   public EntityType EntityType { get; set; }
   public string Url { get; set; }
   public string ContentType { get; set; }
}

public enum EntityType
{
   Ticket,
   Comment
}