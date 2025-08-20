using Domain.Interfaces;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public interface IGenericRepository<T> where T:class, IEntity
{
    Task AddAsync(T entity);
    Task Update(T entity);
    Task Delete(T entity);
    Task<T> GetByIdAsync(int id);
    Task AddRangeAsync(List<T> entities);
}

public class GenericRepository<T>(AppDbContext dbContext): IGenericRepository<T> where T:class, IEntity
{
    public async Task AddRangeAsync(List<T> entities)
    {
        await dbContext.Set<T>().AddRangeAsync(entities);
    }
    public async Task AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity);
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await dbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id)
               ?? throw new Exception($"{typeof(T).Name} with Id = {id} not found");
    }

    public Task Update(T entity)
    {
        dbContext.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public Task Delete(T entity)
    {
        dbContext.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }
}