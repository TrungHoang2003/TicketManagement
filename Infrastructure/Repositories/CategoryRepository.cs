using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>;

public class CategoryRepository(AppDbContext dbContext) : GenericRepository<Category>(dbContext), ICategoryRepository;