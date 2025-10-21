using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface IHistoryRepository : IGenericRepository<History>
{

}

public class HistoryRepository(AppDbContext dbContext) : GenericRepository<History>(dbContext), IHistoryRepository
{

}