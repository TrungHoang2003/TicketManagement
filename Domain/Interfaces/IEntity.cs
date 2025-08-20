using System.ComponentModel.DataAnnotations;

namespace Domain.Interfaces;

public interface IEntity
{
   public int Id { get; init; }
}

public abstract class Entity: IEntity
{
   [Key]
   public int Id { get; init; }
}