using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<Ticket> GetByIdWithAssigneesAsync(int id);
};

public class TicketRepository(AppDbContext dbContext) : GenericRepository<Ticket>(dbContext), ITicketRepository
{
    public async Task<Ticket> GetByIdWithAssigneesAsync(int id)
    {
        return await dbContext.Set<Ticket>()
            .Include(t => t.Assignees)
            .Include(t => t.Creator)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new Exception($"Ticket with Id = {id} not found");
    }
}