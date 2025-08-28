using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface IProgressRepository: IGenericRepository<Progress>
{
    
}

public class ProgressRepository(AppDbContext dbContext) : GenericRepository<Progress>(dbContext), IProgressRepository
{
    
}