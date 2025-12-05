using Application.DTOs;
using Application.Erros;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Commons;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface ITicketService
{
    Task<Result<DashboardTicketDto>> GetDashBoard();
    Task<Result> Create(CreateTicketRequest createTicketRequest); 
    Task<Result> Update(UpdateTicketRequest updateTicketRequest);
    Task<Result> Assign(AssignTicketRequest assignTicketRequest );
    Task<Result> UnassignEmployee(UnassignEmployeeRequest request);
    Task<Result> RejectTicket(RejectTicketDto rejectTicketDto);
    Task<Result> HandleTicket(int ticketId);
    Task<Result> AddHead(AddHeadRequest addHeadRequest);
    Task<Result> CompleteTicket(CompleteTicketDto completeTicketDto);
    Task<Result<TicketDetailDto>> GetDetailTicket(int ticketId);
    Task<Result<GetListTicketResponse>> GetListTicket(GetListTicketRequest request);
};

public class TicketService(ICloudinaryService cloudinary, IUnitOfWork unitOfWork,
    IEmailBackgroundService emailBackgroundService, IUserService userService, IMapper mapper, AppDbContext dbContext) : ITicketService
{
    public async Task<Result<DashboardTicketDto>> GetDashBoard()
    {
        var currentLoginUserId = userService.GetLoginUserId();

        var totalCreatedTickets = await unitOfWork.Ticket
            .GetAll().Where(t => t.CreatorId == currentLoginUserId).CountAsync();

        var totalFollowingTickets = await unitOfWork.Ticket
            .GetAll()
            .Include(t => t.Heads)
            .Where(t => t.Heads.Any(h => h.HeadId == currentLoginUserId))
            .CountAsync();

        var totalAssignedTickets = await unitOfWork.Ticket
            .GetAll()
            .Include(t => t.Assignees)
            .Where(t => t.Assignees.Any(a => a.AssigneeId == currentLoginUserId))
            .CountAsync();

        var query = unitOfWork.Ticket
            .GetAll()
            .Include(t => t.Assignees)
            .Include(t => t.Heads)
            .Where(t => t.Assignees.Any(ta => ta.AssigneeId == currentLoginUserId)
                        || t.Heads.Any(th => th.HeadId == currentLoginUserId)
                        || t.CreatorId == currentLoginUserId);
        
        var totalUnreceivedTickets = await query.Where(t=>t.Status == Status.Pending).CountAsync();
        var totalRejectedTickets = await query.Where(t => t.Status == Status.Rejected).CountAsync();
        var totalCompletedTickets = await query.Where(t => t.Status == Status.Closed).CountAsync();
        var totalClosedTickets = await query.Where(t => t.Status == Status.Closed).CountAsync();
        var totalInprogressTickets = await query.Where(t => t.Status == Status.InProgress).CountAsync();
            
        var totalTickets = totalAssignedTickets + totalFollowingTickets + totalCreatedTickets;

        // Lấy thống kê theo CauseType
        var ticketsByCauseType = await query
            .Include(t => t.CauseType)
            .Where(t => t.CauseTypeId != null)
            .GroupBy(t => new { t.CauseTypeId, t.CauseType!.Name })
            .Select(g => new CauseTypeStatDto
            {
                CauseTypeName = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        // Lấy thống kê theo Category
        var ticketsByCategory = await query
            .Include(t => t.Category)
            .GroupBy(t => new { t.CategoryId, t.Category.Name })
            .Select(g => new CategoryStatDto
            {
                CategoryName = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        // Lấy thống kê theo Priority
        var ticketsByPriority = await query
            .GroupBy(t => t.Priority)
            .Select(g => new PriorityStatDto
            {
                Priority = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync();

        // Lấy thống kê theo tháng (6 tháng gần nhất)
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var ticketsByMonth = await unitOfWork.Ticket
            .GetAll()
            .Where(t => t.CreatedAt >= sixMonthsAgo)
            .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Created = g.Count(),
                Completed = g.Count(t => t.Status == Status.Closed),
                InProgress = g.Count(t => t.Status == Status.InProgress)
            })
            .ToListAsync();

        var ticketsByMonthDto = ticketsByMonth
            .Select(x => new StatusTimelineDto
            {
                Month = $"{x.Year}-{x.Month:D2}",
                Created = x.Created,
                Completed = x.Completed,
                InProgress = x.InProgress
            })
            .OrderBy(x => x.Month)
            .ToList();

        // Tính toán hiệu suất
        var completedTickets = await query
            .Where(t => t.Status == Status.Closed)
            .Include(t => t.Progresses)
            .ToListAsync();

        var performance = new PerformanceStatDto();
        
        if (completedTickets.Any())
        {
            // Tính số ngày hoàn thành trung bình
            var completionDays = completedTickets
                .Where(t => t.ExpectedCompleteDate.HasValue)
                .Select(t => 
                {
                    var completedDate = t.Progresses
                        .Where(p => p.Note.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase) || 
                                   p.Note.Contains("đóng", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(p => p.Date)
                        .FirstOrDefault()?.Date ?? DateTime.UtcNow;
                    return (completedDate - t.CreatedAt).TotalDays;
                })
                .ToList();

            performance.AverageCompletionDays = completionDays.Any() 
                ? Math.Round(completionDays.Average(), 2) 
                : 0;

            // Đếm số ticket hoàn thành đúng hạn và trễ hạn
            performance.OnTimeTickets = completedTickets.Count(t => 
            {
                if (!t.ExpectedCompleteDate.HasValue) 
                    return true;
                
                var deadline = t.ExpectedCompleteDate ?? t.DesiredCompleteDate;
                var completedDate = t.Progresses
                    .Where(p => p.Note.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase) || 
                               p.Note.Contains("đóng", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefault()?.Date ?? DateTime.UtcNow;
                
                return completedDate <= deadline;
            });

            performance.OverdueTickets = completedTickets.Count - performance.OnTimeTickets;

            // Tính tỷ lệ hoàn thành
            var allTicketsCount = await query.CountAsync();
            performance.CompletionRate = allTicketsCount > 0 
                ? Math.Round((double)completedTickets.Count / allTicketsCount * 100, 2) 
                : 0;
        }

        var dashboardTicketDto = new DashboardTicketDto
        {
            TotalTicket = totalTickets,
            TotalCreatedTicket = totalCreatedTickets,
            TotalFollowingTicket = totalFollowingTickets,
            TotalAssignedTicket = totalAssignedTickets,
            TotalUnReceivedTicket = totalUnreceivedTickets,
            TotalRejectedTicket = totalRejectedTickets,
            TotalCompletedTicket =  totalCompletedTickets,
            TotalInProgressTicket = totalInprogressTickets,
            TotalClosedTicket = totalClosedTickets,
            TicketsByCauseType = ticketsByCauseType,
            TicketsByCategory = ticketsByCategory,
            TicketsByPriority = ticketsByPriority,
            TicketsByMonth = ticketsByMonthDto,
            Performance = performance
        };
        return dashboardTicketDto;

    }

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
            DesiredCompleteDate = createTicketRequest.DesiredCompleteDate,
        };
        
        var progress = new Progress
        {
            TicketStatus = ticket.Status.ToString(),
            EmployeeName = creator.FullName,
            Note = "đã tạo yêu cầu"
        };
        
        ticket.Progresses.Add(progress);
        
        // Add the head of department as the main head for this ticket
        var ticketHead = new TicketHead
        {
            HeadId = headOfDepartment.Id,
            Ticket = ticket,
            IsMainHead = true
        };
        ticket.Heads.Add(ticketHead);
        
        await unitOfWork.Ticket.AddAsync(ticket);
        await unitOfWork.SaveChangesAsync();
        
        if (createTicketRequest.Base64Files != null && createTicketRequest.Base64Files.Count != 0)
        {
            var fileNames = createTicketRequest.FileNames ?? 
                            Enumerable.Range(0, createTicketRequest.Base64Files.Count)
                                      .Select(i => $"file_{i}")
                                      .ToList();
            
            var result = await cloudinary.UploadFiles(createTicketRequest.Base64Files, fileNames);
            var attachmentList = new List<Attachment>();
            
            for (int i = 0; i < result.Count; i++)
            {
                var attachment = new Attachment
                {
                    EntityId = ticket.Id,
                    EntityType = EntityType.Ticket,
                    Url = result[i],
                    ContentType = "file",
                    FileName = i < fileNames.Count ? fileNames[i] : $"file_{i}"
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

    public async Task<Result> Update(UpdateTicketRequest updateTicketRequest)
    {
        var ticket = await unitOfWork.Ticket.GetByIdAsync(updateTicketRequest.TicketId);

        var causeType = await unitOfWork.CauseType.GetByIdAsync(updateTicketRequest.CauseTypeId);
        
        ticket.CauseType = causeType;
        ticket.CauseTypeId = updateTicketRequest.CauseTypeId;
        ticket.ImplementationPlan = updateTicketRequest.ImplementationPlan;
        ticket.Cause = updateTicketRequest.Cause;
        ticket.ExpectedStartDate = updateTicketRequest.ExpectedStartDate;
        ticket.ExpectedCompleteDate = updateTicketRequest.ExpectedCompleteDate;
        
        // Handle file deletions
        if (updateTicketRequest.FilesToDelete != null && updateTicketRequest.FilesToDelete.Count > 0)
        {
            // Get existing ticket attachments
            var existingAttachments = unitOfWork.Attachment.GetAll()
                .Where(a => a.EntityId == ticket.Id && a.EntityType == EntityType.Ticket)
                .ToList();
            
            // Delete attachments by index
            foreach (var index in updateTicketRequest.FilesToDelete.OrderByDescending(i => i))
            {
                if (index >= 0 && index < existingAttachments.Count)
                {
                    await unitOfWork.Attachment.Delete(existingAttachments[index]);
                }
            }
        }
        
        // Handle new file uploads
        if (updateTicketRequest.Base64Files != null && updateTicketRequest.Base64Files.Count > 0)
        {
            var uploadedUrls = await cloudinary.UploadFiles(updateTicketRequest.Base64Files, updateTicketRequest.FileNames);
            
            // Create new attachments
            for (int i = 0; i < uploadedUrls.Count; i++)
            {
                var attachment = new Attachment
                {
                    EntityId = ticket.Id,
                    EntityType = EntityType.Ticket,
                    Url = uploadedUrls[i],
                    FileName = updateTicketRequest.FileNames?[i] ?? $"file_{i}",
                    ContentType = "application/octet-stream" // Default content type
                };
                await unitOfWork.Attachment.AddAsync(attachment);
            }
        }
        
        await unitOfWork.SaveChangesAsync();
        
        return Result.IsSuccess();
    }

    public async Task<Result> Assign(AssignTicketRequest assignTicketRequest)
    {
        var currentLoginUserId = userService.GetLoginUserId();
        var ticket = await unitOfWork.Ticket.GetByIdWithAssigneesAsync(assignTicketRequest.TicketId);
        var head = await unitOfWork.User.FindByIdAsync(currentLoginUserId);
        
        var assignees = await unitOfWork.User.FindByIdsAsync(assignTicketRequest.AssigneeIds);

        if (ticket.Assignees.Count == 0)
            ticket.Status = Status.InProgress;
        
        // Get list of newly added assignees (filter out duplicates)
        var newlyAddedAssignees = new List<User>();
        
        foreach (var assignee in assignees)
        {
            // Check if assignee is already assigned to this ticket
            var isAlreadyAssigned = ticket.Assignees.Any(ta => ta.AssigneeId == assignee.Id);
            
            if (!isAlreadyAssigned)
            {
                var ticketAssignee = new TicketAssignee
                {
                    AssigneeId = assignee.Id,
                    TicketId = ticket.Id,
                    Assignee = assignee
                };
                ticket.Assignees.Add(ticketAssignee);
                newlyAddedAssignees.Add(assignee);
            }
        }
        
        // If no new assignees were added, return early
        if (newlyAddedAssignees.Count == 0)
        {
            return Result.Failure(TicketErrors.AssigneesAlreadyAssigned);
        }
        
        var newAssigneeNames = string.Join(", ", newlyAddedAssignees.Select(a => a.FullName));
        var progress = new Progress
        {
            EmployeeName = head.FullName,
            TicketStatus = ticket.Status.ToString(),
            Note = $"Trưởng phòng ban phân công {newAssigneeNames} xử lý ticket"
        };
        
        ticket.Progresses.Add(progress);
        await unitOfWork.SaveChangesAsync();

        // Gửi email cho assignees mới
        foreach (var assignee in newlyAddedAssignees) 
        {
            var sendTicketDto = new SendTicketEmailDto
            {
                Header = EmailHeader.Assigned,
                TicketId = ticket.Id,
                CreatorName = ticket.Creator.FullName,
                ReceiverEmail = assignee.Email!,
                ReceiverName = assignee.FullName,
                TicketTitle = ticket.Title,
                Note = assignTicketRequest.Note,
                Priority = ticket.Priority.ToString() 
            };
            await emailBackgroundService.QueueEmailAsync(sendTicketDto);
        }
        
        return Result.IsSuccess();
    }

    public async Task<Result> UnassignEmployee(UnassignEmployeeRequest request)
    {
        var currentLoginUserId = userService.GetLoginUserId();
        var ticket = await unitOfWork.Ticket.GetByIdWithAssigneesAsync(request.TicketId);
        var head = await unitOfWork.User.FindByIdAsync(currentLoginUserId);
        
        if (ticket == null)
            return TicketErrors.TicketNotFound;
        
        // Find the assignee to remove
        var assigneeToRemove = ticket.Assignees.FirstOrDefault(ta => ta.AssigneeId == request.EmployeeId);
        
        if (assigneeToRemove == null)
            return new Error("EmployeeNotAssigned", "Nhân viên này không được phân công cho ticket");
        
        // Get employee info before removing
        var employee = await unitOfWork.User.FindByIdAsync(request.EmployeeId);
        
        // Remove the assignee
        ticket.Assignees.Remove(assigneeToRemove);
        
        // Add progress log
        var progress = new Progress
        {
            EmployeeName = head.FullName,
            TicketStatus = ticket.Status.ToString(),
            Note = $"Đã gỡ {employee.FullName} khỏi ticket"
        };
        
        ticket.Progresses.Add(progress);
        await unitOfWork.SaveChangesAsync();
        
        return Result.IsSuccess();
    }

    public async Task<Result> RejectTicket(RejectTicketDto rejectTicketDto)
    {
        var ticket = await unitOfWork.Ticket.GetByIdAsync(rejectTicketDto.TicketId);
        
        var currentLoginUserId = userService.GetLoginUserId();
        var currentHead = await unitOfWork.User.FindByIdAsync(currentLoginUserId);
 
        var progress = new Progress
        {
            EmployeeName = currentHead.FullName,
            TicketStatus = ticket.Status.ToString(),
            Note = $"Trưởng phòng ban từ chối với lý do: {rejectTicketDto.Reason}"
        }; 
        
        ticket.Status = Status.Rejected;
        
        ticket.Progresses.Add(progress);
        await unitOfWork.SaveChangesAsync();
        
        return Result.IsSuccess();
    }

    public async Task<Result> HandleTicket(int ticketId)
    {
        var ticket = await unitOfWork.Ticket.GetByIdAsync(ticketId);
        var currentLoginUserId = userService.GetLoginUserId();
        var currentUser =  await unitOfWork.User.FindByIdAsync(currentLoginUserId);
        
        if(ticket.Assignees.Count == 0)
            ticket.Status = Status.Received;

        var progress = new Progress
        {
            EmployeeName = currentUser.FullName,
            TicketStatus = ticket.Status.ToString(),
            Note = $"Trưởng phòng ban đã tiếp nhận ticket"
        }; 
        
        var ticketHead = new TicketHead
        {
            HeadId = currentLoginUserId,
            Ticket = ticket,
            IsMainHead = true
        }; ticket.Heads.Add(ticketHead);
        
        ticket.Progresses.Add(progress);
        
        await unitOfWork.SaveChangesAsync();

        return Result.IsSuccess();
    }

    public async Task<Result> AddHead(AddHeadRequest addHeadRequest)
    {
        var ticket = await unitOfWork.Ticket
            .GetAll()
            .Include(t=>t.Heads)
            .FirstOrDefaultAsync(t => t.Id == addHeadRequest.TicketId);

        if (ticket == null)
            return TicketErrors.TicketNotFound;
        
        var currentLoginUserId = userService.GetLoginUserId();
        var currentUser =  await unitOfWork.User.FindByIdAsync(currentLoginUserId);
        
        var heads = new List<User>();
        
        foreach (var headId in addHeadRequest.HeadIds)
        {
            var head = await unitOfWork.User.FindByIdAsync(headId);
            if (head != null)
            {
                heads.Add(head);
                
                // Add head to ticket's Heads collection if not already added
                if (!ticket.Heads.Any(th => th.HeadId == headId))
                {
                    ticket.Heads.Add(new TicketHead
                    {
                        TicketId = ticket.Id,
                        HeadId = headId,
                        IsMainHead = false
                    });
                }
            }
        }
        
        var headNames = heads.Count > 0 
            ? string.Join(", ", heads.Select(h => h.FullName)) 
            : "Trưởng phòng";
        
        var progress = new Progress
        {
            EmployeeName = currentUser.FullName,
            TicketStatus = ticket.Status.ToString(),
            Note = $"Request sự hỗ trợ từ trưởng phòng ban {headNames}"
        }; 
        
        ticket.Progresses.Add(progress);
        await unitOfWork.SaveChangesAsync();

        // Gửi email cho tất cả Head request được thêm 
        foreach (var head in heads ) 
        {
            var sendTicketDto = new SendTicketEmailDto
            {
                Header = EmailHeader.Created,
                TicketId = ticket.Id,
                CreatorName = ticket.Creator.FullName,
                ReceiverEmail = head.Email!,
                ReceiverName = head.FullName,
                TicketTitle = ticket.Title,
                Note = addHeadRequest.Note,
                Priority = ticket.Priority.ToString() 
            };
            await emailBackgroundService.QueueEmailAsync(sendTicketDto);
        }
        return Result.IsSuccess();
    }

    public async Task<Result> CompleteTicket(CompleteTicketDto completeTicketDto)
    {
        var ticket = await unitOfWork.Ticket
            .GetAll()
            .Include(t => t.Progresses)
            .Include(t => t.Creator)
            .FirstOrDefaultAsync(t => t.Id == completeTicketDto.TicketId);

        if (ticket == null)
            return TicketErrors.TicketNotFound;
        
        var currentLoginUserId = userService.GetLoginUserId();
        var currentUser = await unitOfWork.User.FindByIdAsync(currentLoginUserId);
        
        // Create progress entry for completion
        var progress = new Progress
        {
            EmployeeName = currentUser.FullName,
            TicketStatus = Status.Closed.ToString(),
            Note = $"Đóng phiếu với ghi chú: {completeTicketDto.CompletionNote}"
        };
        
        ticket.Status = Status.Closed;
        ticket.CompletedAt = DateTime.UtcNow; // Cập nhật ngày hoàn thành
        ticket.Progresses.Add(progress);
        
        await unitOfWork.SaveChangesAsync();
        
        return Result.IsSuccess();
    }

    public async Task<Result<TicketDetailDto>> GetDetailTicket(int ticketId)
    {
        var ticket = await unitOfWork.Ticket.GetAll()
            .Where(t => t.Id == ticketId)
            .Include(t => t.Assignees)
            .ThenInclude(a => a.Assignee)
            .Include(t => t.Heads)
            .ThenInclude(h => h.Head)
            .Include(t => t.Category)
            .Include(t => t.Creator)
            .Include(t => t.CauseType)
            .FirstOrDefaultAsync();
        
        if (ticket == null)
            return new Error("NotFound", $"Ticket with id {ticketId} not found");
        
        var ticketDto = mapper.Map<TicketDetailDto>(ticket);
        
        var attachments = await unitOfWork.Attachment.GetAll()
            .Where(a => a.EntityType == EntityType.Ticket && a.EntityId == ticketId)
            .ToListAsync();
        
        ticketDto.FileUrls = attachments.Select(a => a.Url).ToList();
        ticketDto.FileNames = attachments.Select(a => a.FileName ?? "file").ToList();
        
        // Get all progresses for timeline
        var progresses = await unitOfWork.Progress.GetAll()
            .Where(p => p.TicketId == ticketId)
            .OrderBy(p => p.Id)
            .ProjectTo<ProgressDto>(mapper.ConfigurationProvider)
            .ToListAsync();
        
        ticketDto.Progresses = progresses;
        
        // Get heads list with IsMainHead from database
        var ticketHeads = await unitOfWork.Ticket.GetAll()
            .Where(t => t.Id == ticketId)
            .SelectMany(t => t.Heads)
            .ProjectTo<HeadDto>(mapper.ConfigurationProvider)
            .ToListAsync();
        
        ticketDto.Heads = ticketHeads;
        
        // Get rejection reason if ticket is rejected
        if (ticketDto.Status == "Rejected")
        {
            var rejectionProgress = progresses
                .Where(p => p.Note.Contains("từ chối"))
                .OrderByDescending(p => p.Date)
                .FirstOrDefault();
            
            ticketDto.RejectionReason = rejectionProgress?.Note;
        }
        
        // Get completion note if ticket is completed/closed
        if (ticketDto.Status == "Completed" || ticketDto.Status == "Closed")
        {
            var completionProgress = progresses
                .Where(p => p.Note.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase) || 
                           p.Note.Contains("đóng", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.Date)
                .FirstOrDefault();
            
            ticketDto.CompletionNote = completionProgress?.Note;
        }
        
        return ticketDto;
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
        
        if (request.IsFollowing.HasValue && request.IsFollowing.Value)
        {
            query = query.Where(t => t.Heads.Any(a => a.HeadId== loginUserId));
        } 
        
        if (request.IsCreated.HasValue && request.IsCreated.Value)
        {
            query = query.Where(t => t.CreatorId == loginUserId);
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

        query = query.Include(t => t.Category).Include(t => t.Assignees)
            .ThenInclude(a => a.Assignee)
            .Include(t => t.Heads)
            .ThenInclude(h => h.Head);
        
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
    
    public static string GetHeadNames(List<TicketHead>? heads)
    {
        if (heads == null || heads.Count == 0)
            return "Chưa có người nhận";
        
        var names =heads 
            .Select(a => a.Head.UserName)
            .ToList();
        
        return string.Join(", ", names);
    }
    
    public static string GetAssigneeNames(List<TicketAssignee>? assignees)
    {
        if (assignees == null || assignees.Count == 0)
            return "Chưa có người nhận";
        
        var names = assignees
            .Select(a => a.Assignee.UserName)
            .ToList();
        
        return string.Join(", ", names);
    }
}
