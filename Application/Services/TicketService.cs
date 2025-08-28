using Application.DTOs;
using Application.Erros;
using BuildingBlocks.Commons;
using BuildingBlocks.EmailHelper;
using Domain.Entities;
using Infrastructure.Background;
using Infrastructure.Repositories;

namespace Application.Services;

public interface ITicketService
{
    Task<Result> Create(CreateTicketDto createTicketDto);
    Task<Result> Assign(AssignDto assignDto, int currentLoginUserId);
    Task<Result> RejectTicket(RejectTicketDto rejectTicketDto);
};

public class TicketService(ICloudinaryService cloudinary, IUnitOfWork unitOfWork,
    IEmailBackgroundService emailBackgroundService) : ITicketService
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
        
        var progress = new Progress
        {
            Step = 1,
            TicketStatus = ticket.Status.ToString(),
            EmployeeName = creator.FullName,
            Note = $"{creator.FullName} đã tạo yêu cầu"
        };
        ticket.Progresses.Add(progress);
        await unitOfWork.Ticket.AddAsync(ticket);
        await unitOfWork.SaveChangesAsync();
        
        if (createTicketDto.Base64Files != null && createTicketDto.Base64Files.Count != 0)
        {
            var result = await cloudinary.UploadFiles(createTicketDto.Base64Files);
            var attachmentList = new List<Attachment>();
            
            foreach (var url in result)
            {
                var attachment = new Attachment
                {
                    EntityId = ticket.Id,
                    EntityType = EntityType.Ticket,
                    Url = url,
                    ContentType = "file"
                }; 
                attachmentList.Add(attachment);
            }
            await unitOfWork.Attachment.AddRangeAsync(attachmentList);
        }
        await unitOfWork.SaveChangesAsync();

        var sendTicketDto = new SendTicketEmailDto
        {
            Header = EmailHeader.Created,
            TicketId = ticket.Id,
            CreatorName = creator.FullName,
            ReceiverEmail = headOfDepartment.Email!,
            ReceiverName = headOfDepartment.FullName,
            TicketTitle = ticket.Title,
            Priority = ticket.Priority.ToString() 
        };
        
        // Queue email task với EmailBackgroundService
        await emailBackgroundService.QueueEmailAsync(sendTicketDto);

        return Result.IsSuccess();
    }

    public async Task<Result> Assign(AssignDto assignDto, int currentLoginUserId)
    {
        var ticket = await unitOfWork.Ticket.GetByIdAsync(assignDto.TicketId);
        if (ticket.AssigneeId != null) return new Error("Business Error", "This ticket is already assigned");
        
        var headOfDepartment = await unitOfWork.User.FindByIdAsync(ticket.HeadDepartmentId);

        if (currentLoginUserId != headOfDepartment.Id)
            return AuthenErrors.NotAuthorized;
        
        var assignee = await unitOfWork.User.FindByIdAsync(assignDto.AssigneeId);
        if (assignee.DepartmentId != headOfDepartment.DepartmentId)
            return new Error("Business Error", "Chosen employee does not belong to relative department");
                
        ticket.Assignee = assignee;
        ticket.Status = Status.Received;

        var progress = new Progress
        {
            Step = 2,
            EmployeeName = headOfDepartment.UserName!,
            TicketStatus = ticket.Status.ToString(),
            Note = $"{ticket.HeadOfDepartment.UserName} Received request"
        };
        
        ticket.Progresses.Add(progress);
        await unitOfWork.SaveChangesAsync();
        
        var sendTicketDto = new SendTicketEmailDto
        {
            Header = EmailHeader.Assigned,
            TicketId = ticket.Id,
            CreatorName = ticket.Creator.FullName,
            ReceiverEmail = assignee.Email!,
            ReceiverName = assignee.FullName,
            TicketTitle = ticket.Title,
            Priority = ticket.Priority.ToString() 
        };
        
        await emailBackgroundService.QueueEmailAsync(sendTicketDto);
        
        return Result.IsSuccess();
    }

    public async Task<Result> RejectTicket(RejectTicketDto rejectTicketDto)
    {
        var ticket = await unitOfWork.Ticket.GetByIdAsync(rejectTicketDto.TicketId);

        if (ticket.Status != Status.Pending)
            return new Error("Business Error", "This ticket is being handled, cannot be rejected");
        
        var headOfDepartment = await unitOfWork.User.FindByIdAsync(ticket.HeadDepartmentId);
        
        var progress = new Progress
        {
            Step = 2,
            EmployeeName = headOfDepartment.UserName!,
            TicketStatus = ticket.Status.ToString(),
            Note = $"{ticket.HeadOfDepartment.UserName} Received request"
        };
        ticket.Status = Status.Rejected;
        ticket.Progresses.Add(progress);
        await unitOfWork.SaveChangesAsync();
        
        var sendTicketDto = new SendTicketEmailDto
        {
            Header = EmailHeader.Rejected,
            TicketId = ticket.Id,
            CreatorName = ticket.Creator.FullName,
            ReceiverEmail = ticket.Creator.Email!,
            ReceiverName = ticket.Creator.FullName,
            TicketTitle = ticket.Title,
            Priority = ticket.Priority.ToString(),
            Reason = rejectTicketDto.Reason
        };
        
        await emailBackgroundService.QueueEmailAsync(sendTicketDto);
        
        return Result.IsSuccess();
    }
}