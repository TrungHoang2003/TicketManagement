using Application.DTOs;
using Application.Erros;
using Application.Mappings;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Commons;
using BuildingBlocks.EmailHelper;
using CloudinaryDotNet.Core;
using Domain.Entities;
using Infrastructure.Background;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface ITicketService
{
    Task<Result> Create(CreateTicketRequest createTicketRequest);
    Task<Result> Assign(AssignTicketRequest assignTicketRequest );
    Task<Result> RejectTicket(RejectTicketDto rejectTicketDto);
    Task<Result> HandleTicket(int ticketId);
    Task<Result> FollowTicket(int ticketId);
    Task<Result<TicketDetailDto>> GetDetailTicket(int ticketId);
    Task<Result<GetListTicketResponse>> GetListTicket(GetListTicketRequest request);
};

public class TicketService(ICloudinaryService cloudinary, IUnitOfWork unitOfWork,
    IEmailBackgroundService emailBackgroundService, IUserService userService, IMapper mapper) : ITicketService
{
    public async Task<Result> Create(CreateTicketRequest createTicketRequest)
    {
        var creator = await unitOfWork.User.FindByIdAsync(userService.GetLoginUserId());
        var category = await unitOfWork.Category.GetByIdAsync(createTicketRequest.CategoryId);

        var headOfDepartment = await unitOfWork.User.GetHeadOfDepartment(category.DepartmentId);
        var stringPriority = createTicketRequest.Priority.ToLowerInvariant();
        var priority = stringPriority switch
        {
            "high" => Priority.High,
            "medium" => Priority.Medium,
            _ => Priority.Low
        };

        var ticket = new Ticket
        {
            Title = createTicketRequest.Title,
            Creator = creator,
            Category = category,
            Priority = priority,
            Content = createTicketRequest.Content,
            HeadOfDepartment = headOfDepartment,
            DesiredCompleteDate = createTicketRequest.DesiredCompleteDate,
        };
        
        var progress = new Progress
        {
            TicketStatus = ticket.Status.ToString(),
            EmployeeName = creator.FullName,
            Note = $"{creator.FullName} đã tạo yêu cầu"
        };
        ticket.Progresses.Add(progress);
        await unitOfWork.Ticket.AddAsync(ticket);
        await unitOfWork.SaveChangesAsync();
        
        if (createTicketRequest.Base64Files != null && createTicketRequest.Base64Files.Count != 0)
        {
            var result = await cloudinary.UploadFiles(createTicketRequest.Base64Files);
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

    public async Task<Result> Assign(AssignTicketRequest assignTicketRequest)
    {
        var currentLoginUserId = userService.GetLoginUserId();
        
        var ticket = await unitOfWork.Ticket.GetByIdAsync(assignTicketRequest.TicketId);

        ticket.Cause = assignTicketRequest.Cause;
        ticket.ImplementationPlanId = assignTicketRequest.ImplementationPlanId;
        ticket.ExpectedStartDate = assignTicketRequest.ExpectedStartDate;
        ticket.ExpectedCompleteDate = assignTicketRequest.ExpectedCompleteDate;
        ticket.CauseTypeId = assignTicketRequest.CauseTypeId;
        
        var headOfDepartment = await unitOfWork.User.FindByIdAsync(ticket.HeadDepartmentId);
        
        if (currentLoginUserId != headOfDepartment.Id)
            return AuthenErrors.NotAuthorized;

        var assignees = await unitOfWork.User.FindByIdsAsync(assignTicketRequest.AssigneeIds);
        
        ticket.Assignees.Clear();
        
        foreach (var assignee in assignees)
        {
            var ticketAssignee = new TicketAssignee
            {
                AssigneeId = assignee.Id,
                TicketId = ticket.Id,
                Assignee = assignee
            };
            ticket.Assignees.Add(ticketAssignee);
        }
        
        ticket.Status = Status.Received;

        var assigneeNames = GetNames(ticket.Assignees);
        var progress = new Progress
        {
            EmployeeName = headOfDepartment.UserName!,
            TicketStatus = ticket.Status.ToString(),
            Note = $"{headOfDepartment.UserName} assigned ticket to {assigneeNames}"
        };
        
        ticket.Progresses.Add(progress);
        await unitOfWork.SaveChangesAsync();

        // Gửi email cho tất cả assignees
        foreach (var assignee in assignees) 
        {
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
        }
        
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

    public async Task<Result> HandleTicket(int ticketId)
    {
        var ticket = await unitOfWork.Ticket.GetByIdAsync(ticketId);

        if (ticket.Assignees.Count == 0) 
            return new Error("Business Error", "This ticket hasn't been assigned yet");
        
        var currentLoginUserId = userService.GetLoginUserId();
        
        var isAssigned = ticket.Assignees.Any(ta => ta.AssigneeId == currentLoginUserId);
        if (!isAssigned)
            return AuthenErrors.NotAuthorized;
        
        ticket.Status = Status.InProgress;

        var progress = new Progress
        {
            EmployeeName = GetNames(ticket.Assignees),
            TicketStatus = ticket.Status.ToString(),
            Note = $"{GetNames(ticket.Assignees)} is handling request"
        };
        ticket.Progresses.Add(progress);
        await unitOfWork.SaveChangesAsync();

        return Result.IsSuccess();
    }

    public async Task<Result> FollowTicket(int ticketId)
    {
        var ticket = await unitOfWork.Ticket.GetByIdAsync(ticketId);
        var userId = userService.GetLoginUserId();
        var user = await unitOfWork.User.FindByIdAsync(userId);
        
        user.FollowingTickets.Add(ticket);
        await unitOfWork.SaveChangesAsync();
        
        return Result.IsSuccess();
    }

    public async Task<Result<TicketDetailDto>> GetDetailTicket(int ticketId)
    {
        var ticket = await unitOfWork.Ticket.GetAll()
            .Where(t => t.Id == ticketId)
            .Include(t => t.Assignees)
            .ThenInclude(a => a.Assignee)
            .Include(t => t.Category)
            .Include(t => t.Creator)
            .Include(t => t.HeadOfDepartment)
            .Include(t => t.CauseType)
            .Include(t => t.ImplementationPlan)
            .ProjectTo<TicketDetailDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        
        if (ticket == null)
            return new Error("NotFound", $"Ticket with id {ticketId} not found");
        
        var fileUrls = await unitOfWork.Attachment.GetAll()
            .Where(a => a.EntityType == EntityType.Ticket && a.EntityId == ticketId)
            .Select(a => a.Url)
            .ToListAsync();
        
        ticket.FileUrls = fileUrls;
        
        return ticket;
    }

    public async Task<Result<GetListTicketResponse>> GetListTicket(GetListTicketRequest request)
    {
        var loginUserId = userService.GetLoginUserId();
        var query = unitOfWork.Ticket
            .GetAll().AsQueryable();

        if (request.IsAssigned.HasValue && request.IsAssigned.Value)
        {
            query = query.Where(t => t.Assignees.Any(a => a.AssigneeId == loginUserId));
        } 
        
        if (request.IsCreated.HasValue && request.IsCreated.Value)
        {
            query = query.Where(t => t.CreatorId == loginUserId);
        }
        
        if (request.IsFollowing.HasValue && request.IsFollowing.Value)
        {
            query = query.Where(t => t.HeadDepartmentId== loginUserId);
        }

        if (!string.IsNullOrEmpty(request.Title))
        {
            query = query.Where(t => t.Title.Contains(request.Title));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(t => t.Status.ToString().ToLower() == request.Status.ToLower());
        }
        
        if (!string.IsNullOrEmpty(request.Prority))
        {
            query = query.Where(t => t.Priority.ToString().ToLower() == request.Prority.ToLower());
        }
        
        if (request.CreateAt.HasValue)
        {
            query = query.Where(t => t.CreatedAt == request.CreateAt.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);
        }

        query = query.Include(t => t.Category).
            Include(t => t.Assignees)
            .ThenInclude(a => a.Assignee);
        
        var totalCount = await query.CountAsync();
        
        var tickets = await query
            .OrderBy(u => u.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<TicketDto>(mapper.ConfigurationProvider)
            .ToListAsync();
        
        var getListTicketResponse = new GetListTicketResponse
        {
            Tickets = tickets,
            TotalCount = totalCount
        };

        return getListTicketResponse;
    }
    
    public static string GetNames(List<TicketAssignee>? assignees)
    {
        if (assignees == null || assignees.Count == 0)
            return "Chưa có người nhận";
        
        var names = assignees
            .Select(a => a.Assignee.UserName)
            .ToList();
        
        return string.Join(", ", names);
    }
}
