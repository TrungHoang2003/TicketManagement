using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{

}

public class ProjectRepository(AppDbContext dbContext) : GenericRepository<Project>(dbContext), IProjectRepository
{

}