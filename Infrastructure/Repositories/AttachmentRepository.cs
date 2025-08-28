
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface IAttachmentRepository: IGenericRepository<Attachment>
{
    
}

public class AttachmentRepository(AppDbContext dbContext) : GenericRepository<Attachment>(dbContext), IAttachmentRepository
{
    
}