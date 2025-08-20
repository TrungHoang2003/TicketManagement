using Dapper;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface IDepartmentRepository: IGenericRepository<Department>
{
}

public class DepartmentRepository(AppDbContext dbContext, PostgreSqlServer server) :GenericRepository<Department>(dbContext), IDepartmentRepository
{
}