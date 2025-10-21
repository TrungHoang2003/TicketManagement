using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface ICauseTypeRepository : IGenericRepository<CauseType>
{

}

public class CauseTypeRepository(AppDbContext dbContext) : GenericRepository<CauseType>(dbContext), ICauseTypeRepository
{

}