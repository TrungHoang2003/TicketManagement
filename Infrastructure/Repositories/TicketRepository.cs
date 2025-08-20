using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface ITicketRepository : IGenericRepository<Ticket>;

public class TicketRepository(AppDbContext dbContext) : GenericRepository<Ticket>(dbContext), ITicketRepository
{
    
}