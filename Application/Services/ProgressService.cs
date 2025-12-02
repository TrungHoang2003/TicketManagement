using Application.DTOs;
using Application.Erros;
using AutoMapper;
using BuildingBlocks.Commons;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface IProgressService
{
    Task<Result<List<ProgressDto>>> GetTicketProgresses(int ticketId);
}

class ProgressService(IUnitOfWork unitOfWork, IMapper mapper): IProgressService
{
    public async Task<Result<List<ProgressDto>>> GetTicketProgresses(int ticketId)
    {
        var exists = await unitOfWork.Ticket.ExistsAsync(ticketId);
        if (!exists)
            return TicketErrors.TicketNotFound;
        
        var progresses = await unitOfWork.Progress.GetAll()
            .Include(p => p.Ticket)
            .Where(p => p.TicketId == ticketId)
            .OrderBy(p=>p.Date)
            .ToListAsync();
        
        var progressDtos = mapper.Map<List<ProgressDto>>(progresses);
        
        for(var i =0; i<progressDtos.Count; i++)
        {
            progressDtos[i].StepNumber = i + 1;
        }

        return progressDtos;
    }
}