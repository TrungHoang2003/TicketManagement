using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface IUnitOfWork
{
   ITicketRepository Ticket { get; }
   IUserRepository User { get; }
   IDepartmentRepository Department { get; }
   ICategoryRepository Category { get; }
   IAttachmentRepository Attachment { get; }
   Task SaveChangesAsync();
}

public class UnitOfWork(AppDbContext dbContext, ITicketRepository ticket, IUserRepository user, IDepartmentRepository department, ICategoryRepository category, IAttachmentRepository attachment) : IUnitOfWork
{
   public ITicketRepository Ticket { get; } = ticket;
   public IUserRepository User { get; } = user;
   public IDepartmentRepository Department { get; } = department;
   public ICategoryRepository Category { get; } = category;
   public IAttachmentRepository Attachment { get; } = attachment;

   public async Task SaveChangesAsync()
   {
      await dbContext.SaveChangesAsync();
   }
}