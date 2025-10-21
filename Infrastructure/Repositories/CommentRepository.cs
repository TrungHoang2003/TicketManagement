using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface ICommentRepository : IGenericRepository<Comment>
{

}

public class CommentRepository(AppDbContext dbContext) : GenericRepository<Comment>(dbContext), ICommentRepository
{

}