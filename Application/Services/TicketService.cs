using Application.DTOs;
using BuildingBlocks.Commons;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public interface ITicketService
{
    Task<Result> Create(CreateTicketDto createTicketDto);
};

public class TicketService(IUnitOfWork unitOfWork, IEmailService emailService): ITicketService
{
    public async Task<Result> Create(CreateTicketDto createTicketDto)
    {
        var creator = await unitOfWork.User.FindByIdAsync(createTicketDto.CreatorId);
        var category = await unitOfWork.Category.GetByIdAsync(createTicketDto.CategoryId);

        var departmentName = category.Department switch
        {
            DepartmentEnum.Ad => "AD",
            DepartmentEnum.It => "IT",
            DepartmentEnum.Qa => "QA",
            _ => throw new ArgumentOutOfRangeException()
        };

        var headOfDepartment = await unitOfWork.User.GetHeadOfDepartment(departmentName);
        
        var stringPriority = createTicketDto.Priority.ToLowerInvariant();
        
        var priority = stringPriority switch
        {
            "high" => Priority.High,
            "medium" => Priority.Medium,
            _ => Priority.Low
        };

        var ticket = new Ticket
        {
            Title = createTicketDto.Title,
            Creator = creator,
            Category = category,
            Priority = priority,
            Content = createTicketDto.Content,
            HeadOfDepartment = headOfDepartment
        };

        await emailService.SendTicketNotificationAsync(
            headOfDepartment.Email!,
            headOfDepartment.FullName, ticket.Title,
            ticket.Id,
            creator.FullName, stringPriority);

        await unitOfWork.Ticket.AddAsync(ticket);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }
}