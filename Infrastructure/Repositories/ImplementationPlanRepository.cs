using Domain.Entities;
using Humanizer;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface IImplementationPlanRepository : IGenericRepository<ImplementationPlan>
{

}

public class ImplementationPlanRepository(AppDbContext dbContext) : GenericRepository<ImplementationPlan>(dbContext), IImplementationPlanRepository
{

}