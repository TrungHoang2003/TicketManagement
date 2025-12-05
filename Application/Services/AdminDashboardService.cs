using Application.DTOs;
using BuildingBlocks.Commons;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Services;

public interface IAdminDashboardService
{
    Task<Result<AdminDashboardDto>> GetDashboardAsync();
    Task<Result<SystemOverviewDto>> GetSystemOverviewAsync();
    Task<Result<List<EmployeePerformanceDto>>> GetEmployeePerformanceAsync(int? departmentId = null, int topCount = 10);
    Task<Result<List<DepartmentPerformanceDto>>> GetDepartmentPerformanceAsync();
    Task<Result<List<TrendAnalysisDto>>> GetTrendAnalysisAsync(int months = 6);
    Task<Result<List<RealtimeAlertDto>>> GetRealtimeAlertsAsync();
    Task<Result<List<WorkloadAnalysisDto>>> GetWorkloadAnalysisAsync();
    Task<Result<UserDetailStatisticsDto>> GetUserDetailStatisticsAsync(int userId);
    Task<Result<List<EmployeePerformanceDto>>> GetAllUsersStatisticsAsync(int? departmentId = null);
}

public class AdminDashboardService(
    AppDbContext dbContext,
    IUnitOfWork unitOfWork,
    UserManager<User> userManager) : IAdminDashboardService
{
    public async Task<Result<AdminDashboardDto>> GetDashboardAsync()
    {
        try
        {
            var systemOverview = await GetSystemOverviewAsync();
            var topPerformers = await GetEmployeePerformanceAsync(null, 5);
            var underPerformers = await GetEmployeePerformanceAsync(null, -5); // Get bottom 5
            var departmentPerformance = await GetDepartmentPerformanceAsync();
            var trendAnalysis = await GetTrendAnalysisAsync(6);
            var realtimeAlerts = await GetRealtimeAlertsAsync();

            var dashboard = new AdminDashboardDto
            {
                SystemOverview = systemOverview.Data!,
                TopPerformers = topPerformers.Data?.Take(5).ToList() ?? new List<EmployeePerformanceDto>(),
                UnderPerformers = underPerformers.Data?.TakeLast(5).ToList() ?? new List<EmployeePerformanceDto>(),
                DepartmentPerformance = departmentPerformance.Data ?? new List<DepartmentPerformanceDto>(),
                TrendAnalysis = trendAnalysis.Data ?? new List<TrendAnalysisDto>(),
                RealtimeAlerts = realtimeAlerts.Data ?? new List<RealtimeAlertDto>()
            };

            return Result<AdminDashboardDto>.IsSuccess(dashboard);
        }
        catch (Exception ex)
        {
            return Result<AdminDashboardDto>.Failure(new Error("DashboardError", $"Failed to get dashboard: {ex.Message}"));
        }
    }

    public async Task<Result<SystemOverviewDto>> GetSystemOverviewAsync()
    {
        try
        {
            var totalUsers = await dbContext.Users.CountAsync();
            var activeUsers = await dbContext.Users
                .Where(u => u.CreatedTickets.Any(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-30)) ||
                           u.AssignedTickets.Any(a => a.Ticket.CreatedAt >= DateTime.UtcNow.AddDays(-30)))
                .CountAsync();

            var totalDepartments = await unitOfWork.Department.GetAll().CountAsync();
            var totalTickets = await unitOfWork.Ticket.GetAll().CountAsync();

            var ticketStats = await unitOfWork.Ticket.GetAll()
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalPending = ticketStats.FirstOrDefault(t => t.Status == Status.Pending)?.Count ?? 0;
            var totalInProgress = ticketStats.FirstOrDefault(t => t.Status == Status.InProgress)?.Count ?? 0;
            var totalCompleted = ticketStats.FirstOrDefault(t => t.Status == Status.Completed)?.Count ?? 0;
            var totalRejected = ticketStats.FirstOrDefault(t => t.Status == Status.Rejected)?.Count ?? 0;

            var completionRate = totalTickets > 0 ? (double)totalCompleted / totalTickets * 100 : 0;

            // Calculate average resolution days
            var completedTickets = await unitOfWork.Ticket.GetAll()
                .Where(t => t.Status == Status.Completed || t.Status == Status.Closed)
                .Include(t => t.Progresses)
                .ToListAsync();

            var avgResolutionDays = 0.0;
            if (completedTickets.Any())
            {
                var resolutionTimes = completedTickets
                    .Select(t =>
                    {
                        var completedDate = t.Progresses
                            .Where(p => p.Note.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(p => p.Date)
                            .FirstOrDefault()?.Date ?? DateTime.UtcNow;
                        return (completedDate - t.CreatedAt).TotalDays;
                    })
                    .Where(days => days > 0)
                    .ToList();

                avgResolutionDays = resolutionTimes.Any() ? resolutionTimes.Average() : 0;
            }

            // Get Category Statistics - load data first, then group in memory
            var allTicketsForStats = await unitOfWork.Ticket.GetAll()
                .Include(t => t.Category)
                .ToListAsync();

            var categoryStats = allTicketsForStats
                .Where(t => t.Category != null)
                .GroupBy(t => new { t.CategoryId, t.Category!.Name })
                .Select(g => new CategoryStatisticDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    TotalTickets = g.Count(),
                    CompletedTickets = g.Count(t => t.Status == Status.Completed || t.Status == Status.Closed),
                    InProgressTickets = g.Count(t => t.Status == Status.InProgress),
                    PendingTickets = g.Count(t => t.Status == Status.Pending),
                    CompletionRate = g.Count() > 0 ? Math.Round((double)g.Count(t => t.Status == Status.Completed || t.Status == Status.Closed) / g.Count() * 100, 2) : 0
                })
                .OrderByDescending(c => c.TotalTickets)
                .ToList();

            // Get CauseType Statistics - load data first, then group in memory
            var ticketsWithCause = await unitOfWork.Ticket.GetAll()
                .Where(t => t.CauseTypeId.HasValue)
                .Include(t => t.CauseType)
                .ToListAsync();

            var causeTypeStats = ticketsWithCause
                .Where(t => t.CauseType != null)
                .GroupBy(t => new { t.CauseTypeId, t.CauseType!.Name })
                .Select(g => new CauseTypeStatisticDto
                {
                    CauseTypeId = g.Key.CauseTypeId!.Value,
                    CauseTypeName = g.Key.Name,
                    TotalTickets = g.Count(),
                    Percentage = 0 // Will calculate below
                })
                .ToList();

            var totalTicketsWithCause = causeTypeStats.Sum(c => c.TotalTickets);
            foreach (var stat in causeTypeStats)
            {
                stat.Percentage = totalTicketsWithCause > 0 ? Math.Round((double)stat.TotalTickets / totalTicketsWithCause * 100, 2) : 0;
            }
            causeTypeStats = causeTypeStats.OrderByDescending(c => c.TotalTickets).ToList();

            var overview = new SystemOverviewDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalDepartments = totalDepartments,
                TotalTickets = totalTickets,
                TotalPendingTickets = totalPending,
                TotalInProgressTickets = totalInProgress,
                TotalCompletedTickets = totalCompleted,
                TotalRejectedTickets = totalRejected,
                OverallCompletionRate = Math.Round(completionRate, 2),
                AverageResolutionDays = Math.Round(avgResolutionDays, 2),
                CategoryStatistics = categoryStats,
                CauseTypeStatistics = causeTypeStats,
                LastUpdated = DateTime.UtcNow
            };

            return Result<SystemOverviewDto>.IsSuccess(overview);
        }
        catch (Exception ex)
        {
            return Result<SystemOverviewDto>.Failure(new Error("SystemOverviewError", $"Failed to get system overview: {ex.Message}"));
        }
    }

    public async Task<Result<List<EmployeePerformanceDto>>> GetEmployeePerformanceAsync(int? departmentId = null, int topCount = 10)
    {
        try
        {
            var query = dbContext.Users
                .Include(u => u.Department)
                .Include(u => u.AssignedTickets)
                    .ThenInclude(ta => ta.Ticket)
                        .ThenInclude(t => t.Progresses)
                .Where(u => !departmentId.HasValue || u.DepartmentId == departmentId.Value);

            var users = await query.ToListAsync();
            var performances = new List<EmployeePerformanceDto>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var assignedTickets = user.AssignedTickets.Select(ta => ta.Ticket).ToList();

                var totalAssigned = assignedTickets.Count;
                var completed = assignedTickets.Count(t => t.Status == Status.Completed || t.Status == Status.Closed);

                var completionRate = totalAssigned > 0 ? (double)completed / totalAssigned * 100 : 0;

                // Calculate average resolution days
                var completedTickets = assignedTickets.Where(t => t.Status == Status.Completed || t.Status == Status.Closed).ToList();
                var avgResolutionDays = 0.0;
                if (completedTickets.Any())
                {
                    var resolutionTimes = completedTickets
                        .Select(t =>
                        {
                            var completedDate = t.Progresses
                                .Where(p => p.Note.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase))
                                .OrderByDescending(p => p.Date)
                                .FirstOrDefault()?.Date ?? DateTime.UtcNow;
                            return (completedDate - t.CreatedAt).TotalDays;
                        })
                        .Where(days => days > 0)
                        .ToList();

                    avgResolutionDays = resolutionTimes.Any() ? resolutionTimes.Average() : 0;
                }

                var currentWorkload = assignedTickets.Count(t => t.Status == Status.InProgress || t.Status == Status.Pending);

                // Calculate performance rating
                var performanceRating = CalculatePerformanceRating(completionRate, currentWorkload);

                performances.Add(new EmployeePerformanceDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    AssignedTickets = totalAssigned,
                    CompletedTickets = completed,
                    CompletionRate = Math.Round(completionRate, 2),
                    AverageResolutionDays = Math.Round(avgResolutionDays, 2),
                    CurrentWorkload = currentWorkload,
                    PerformanceRating = performanceRating,
                    Roles = roles.ToList()
                });
            }

            // Sort by completion rate
            var sortedPerformances = topCount > 0 
                ? performances.OrderByDescending(p => p.CompletionRate).Take(topCount).ToList()
                : performances.OrderBy(p => p.CompletionRate).Take(Math.Abs(topCount)).ToList();

            return Result<List<EmployeePerformanceDto>>.IsSuccess(sortedPerformances);
        }
        catch (Exception ex)
        {
            return Result<List<EmployeePerformanceDto>>.Failure(new Error("EmployeePerformanceError", $"Failed to get employee performance: {ex.Message}"));
        }
    }

    public async Task<Result<List<DepartmentPerformanceDto>>> GetDepartmentPerformanceAsync()
    {
        try
        {
            var departments = await unitOfWork.Department.GetAll()
                .Include(d => d.Employees)
                    .ThenInclude(e => e.AssignedTickets)
                        .ThenInclude(ta => ta.Ticket)
                            .ThenInclude(t => t.Category)
                .ToListAsync();

            var performances = new List<DepartmentPerformanceDto>();

            foreach (var dept in departments)
            {
                var totalEmployees = dept.Employees.Count;
                var activeEmployees = dept.Employees.Count(e => 
                    e.AssignedTickets.Any(ta => ta.Ticket.CreatedAt >= DateTime.UtcNow.AddDays(-30)));

                var allTickets = dept.Employees
                    .SelectMany(e => e.AssignedTickets.Select(ta => ta.Ticket))
                    .ToList();

                var totalTickets = allTickets.Count;
                var pending = allTickets.Count(t => t.Status == Status.Pending);
                var inProgress = allTickets.Count(t => t.Status == Status.InProgress);
                var completed = allTickets.Count(t => t.Status == Status.Completed || t.Status == Status.Closed);

                var completionRate = totalTickets > 0 ? (double)completed / totalTickets * 100 : 0;

                // Calculate department average resolution time
                var completedTickets = allTickets.Where(t => t.Status == Status.Completed || t.Status == Status.Closed).ToList();
                var avgResolutionDays = 0.0;
                
                if (completedTickets.Any())
                {
                    var resolutionTimes = completedTickets
                        .Where(t => t.Progresses != null && t.Progresses.Any())
                        .Select(t => (DateTime.UtcNow - t.CreatedAt).TotalDays)
                        .ToList();
                    
                    avgResolutionDays = resolutionTimes.Any() ? resolutionTimes.Average() : 0;
                }

                // Workload distribution (average tickets per employee)
                var workloadDistribution = totalEmployees > 0 ? (double)totalTickets / totalEmployees : 0;

                // Top categories for this department
                var topCategories = allTickets
                    .Where(t => t.Category != null)
                    .GroupBy(t => t.Category!.Name)
                    .Select(g => new CategoryStatDto
                    {
                        CategoryName = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(c => c.Count)
                    .Take(3)
                    .ToList();

                var performanceRating = CalculateDepartmentPerformanceRating(completionRate, workloadDistribution);

                performances.Add(new DepartmentPerformanceDto
                {
                    DepartmentId = dept.Id,
                    DepartmentName = dept.Name,
                    TotalEmployees = totalEmployees,
                    ActiveEmployees = activeEmployees,
                    TotalTickets = totalTickets,
                    PendingTickets = pending,
                    InProgressTickets = inProgress,
                    CompletedTickets = completed,
                    CompletionRate = Math.Round(completionRate, 2),
                    AverageResolutionDays = Math.Round(avgResolutionDays, 2),
                    WorkloadDistribution = Math.Round(workloadDistribution, 2),
                    PerformanceRating = performanceRating,
                    TopCategories = topCategories
                });
            }

            return Result<List<DepartmentPerformanceDto>>.IsSuccess(performances);
        }
        catch (Exception ex)
        {
            return Result<List<DepartmentPerformanceDto>>.Failure(new Error("DepartmentPerformanceError", $"Failed to get department performance: {ex.Message}"));
        }
    }

    public async Task<Result<List<TrendAnalysisDto>>> GetTrendAnalysisAsync(int months = 6)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);
            var trends = new List<TrendAnalysisDto>();

            for (int i = 0; i < months; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);

                var monthTickets = await unitOfWork.Ticket.GetAll()
                    .Where(t => t.CreatedAt >= monthStart && t.CreatedAt < monthEnd)
                    .ToListAsync();

                var created = monthTickets.Count;
                var completed = monthTickets.Count(t => t.Status == Status.Completed || t.Status == Status.Closed);
                var inProgress = monthTickets.Count(t => t.Status == Status.InProgress);

                var completionRate = created > 0 ? (double)completed / created * 100 : 0;

                var activeUsers = await dbContext.Users
                    .Where(u => u.CreatedTickets.Any(t => t.CreatedAt >= monthStart && t.CreatedAt < monthEnd) ||
                               u.AssignedTickets.Any(ta => ta.Ticket.CreatedAt >= monthStart && ta.Ticket.CreatedAt < monthEnd))
                    .CountAsync();

                // Calculate average resolution time for completed tickets in this month
                var completedTicketsInMonth = monthTickets.Where(t => t.Status == Status.Completed || t.Status == Status.Closed).ToList();
                var avgResolutionTime = 0.0;

                if (completedTicketsInMonth.Any())
                {
                    var resolutionTimes = completedTicketsInMonth
                        .Select(t => (DateTime.UtcNow - t.CreatedAt).TotalDays)
                        .ToList();
                    avgResolutionTime = resolutionTimes.Average();
                }

                trends.Add(new TrendAnalysisDto
                {
                    Period = monthStart.ToString("yyyy-MM"),
                    TicketsCreated = created,
                    TicketsCompleted = completed,
                    TicketsInProgress = inProgress,
                    CompletionRate = Math.Round(completionRate, 2),
                    AverageResolutionTime = Math.Round(avgResolutionTime, 2),
                    ActiveUsers = activeUsers
                });
            }

            return Result<List<TrendAnalysisDto>>.IsSuccess(trends);
        }
        catch (Exception ex)
        {
            return Result<List<TrendAnalysisDto>>.Failure(new Error("TrendAnalysisError", $"Failed to get trend analysis: {ex.Message}"));
        }
    }

    public async Task<Result<List<RealtimeAlertDto>>> GetRealtimeAlertsAsync()
    {
        try
        {
            var alerts = new List<RealtimeAlertDto>();

            // SLA Violations - tickets overdue
            var overdueTickets = await unitOfWork.Ticket.GetAll()
                .Where(t => t.ExpectedCompleteDate.HasValue && 
                           t.ExpectedCompleteDate.Value < DateTime.UtcNow &&
                           t.Status != Status.Completed && 
                           t.Status != Status.Closed)
                .Include(t => t.Creator)
                .Take(10)
                .ToListAsync();

            foreach (var ticket in overdueTickets)
            {
                alerts.Add(new RealtimeAlertDto
                {
                    Type = "SLA_VIOLATION",
                    Title = "Ticket quá hạn",
                    Description = $"Ticket #{ticket.Id} '{ticket.Title}' đã quá hạn {(DateTime.UtcNow - ticket.ExpectedCompleteDate!.Value).Days} ngày",
                    Severity = "Critical",
                    CreatedAt = DateTime.UtcNow,
                    TicketId = ticket.Id,
                    ActionUrl = $"/tickets/{ticket.Id}"
                });
            }

            // High workload users
            var highWorkloadUsers = await dbContext.Users
                .Include(u => u.AssignedTickets)
                    .ThenInclude(ta => ta.Ticket)
                .Where(u => u.AssignedTickets.Count(ta => 
                    ta.Ticket.Status == Status.InProgress || 
                    ta.Ticket.Status == Status.Pending) > 10)
                .Take(5)
                .ToListAsync();

            foreach (var user in highWorkloadUsers)
            {
                var workload = user.AssignedTickets.Count(ta => 
                    ta.Ticket.Status == Status.InProgress || 
                    ta.Ticket.Status == Status.Pending);

                alerts.Add(new RealtimeAlertDto
                {
                    Type = "HIGH_WORKLOAD",
                    Title = "Tải công việc cao",
                    Description = $"{user.FullName} đang xử lý {workload} tickets",
                    Severity = "Warning",
                    CreatedAt = DateTime.UtcNow,
                    UserId = user.Id,
                    ActionUrl = $"/admin/users/{user.Id}"
                });
            }

            // Pending tickets for too long
            var longPendingTickets = await unitOfWork.Ticket.GetAll()
                .Where(t => t.Status == Status.Pending && 
                           t.CreatedAt < DateTime.UtcNow.AddDays(-3))
                .Take(10)
                .ToListAsync();

            foreach (var ticket in longPendingTickets)
            {
                alerts.Add(new RealtimeAlertDto
                {
                    Type = "LONG_PENDING",
                    Title = "Ticket chờ xử lý lâu",
                    Description = $"Ticket #{ticket.Id} đã chờ xử lý {(DateTime.UtcNow - ticket.CreatedAt).Days} ngày",
                    Severity = "Warning",
                    CreatedAt = DateTime.UtcNow,
                    TicketId = ticket.Id,
                    ActionUrl = $"/tickets/{ticket.Id}"
                });
            }

            return Result<List<RealtimeAlertDto>>.IsSuccess(alerts.OrderByDescending(a => a.CreatedAt).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<RealtimeAlertDto>>.Failure(new Error("RealtimeAlertsError", $"Failed to get realtime alerts: {ex.Message}"));
        }
    }

    public async Task<Result<List<WorkloadAnalysisDto>>> GetWorkloadAnalysisAsync()
    {
        try
        {
            var users = await dbContext.Users
                .Include(u => u.Department)
                .Include(u => u.AssignedTickets)
                    .ThenInclude(ta => ta.Ticket)
                .ToListAsync();

            var workloadAnalysis = users.Select(user =>
            {
                var currentTickets = user.AssignedTickets.Count(ta => 
                    ta.Ticket.Status == Status.InProgress || 
                    ta.Ticket.Status == Status.Pending);

                var highPriorityTickets = user.AssignedTickets.Count(ta => 
                    (ta.Ticket.Priority == Priority.High || ta.Ticket.Priority == Priority.Critical) && 
                    (ta.Ticket.Status == Status.InProgress || ta.Ticket.Status == Status.Pending));

                // Calculate workload score (weighted)
                var workloadScore = (currentTickets * 1.0) + (highPriorityTickets * 2.0);

                var workloadLevel = workloadScore switch
                {
                    <= 3 => "Light",
                    <= 8 => "Normal", 
                    <= 15 => "Heavy",
                    _ => "Overloaded"
                };

                return new WorkloadAnalysisDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    CurrentTickets = currentTickets,
                    HighPriorityTickets = highPriorityTickets,
                    WorkloadScore = Math.Round(workloadScore, 2),
                    WorkloadLevel = workloadLevel
                };
            })
            .OrderByDescending(w => w.WorkloadScore)
            .ToList();

            return Result<List<WorkloadAnalysisDto>>.IsSuccess(workloadAnalysis);
        }
        catch (Exception ex)
        {
            return Result<List<WorkloadAnalysisDto>>.Failure(new Error("WorkloadAnalysisError", $"Failed to get workload analysis: {ex.Message}"));
        }
    }

    private static string CalculatePerformanceRating(double completionRate, int currentWorkload)
    {
        var score = completionRate - (currentWorkload * 2);
        
        return score switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Average",
            _ => "Poor"
        };
    }

    private static string CalculateDepartmentPerformanceRating(double completionRate, double workloadDistribution)
    {
        var score = completionRate - (workloadDistribution * 2);
        
        return score switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good", 
            >= 40 => "Average",
            _ => "Poor"
        };
    }

    public async Task<Result<UserDetailStatisticsDto>> GetUserDetailStatisticsAsync(int userId)
    {
        try
        {
            var user = await dbContext.Users
                .Include(u => u.Department)
                .Include(u => u.AssignedTickets)
                    .ThenInclude(ta => ta.Ticket)
                        .ThenInclude(t => t.Category)
                .Include(u => u.AssignedTickets)
                    .ThenInclude(ta => ta.Ticket)
                        .ThenInclude(t => t.CauseType)
                .Include(u => u.AssignedTickets)
                    .ThenInclude(ta => ta.Ticket)
                        .ThenInclude(t => t.Progresses)
                .Include(u => u.CreatedTickets)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Result<UserDetailStatisticsDto>.Failure(new Error("UserNotFound", $"User with ID {userId} not found"));
            }

            var roles = await userManager.GetRolesAsync(user);
            var assignedTickets = user.AssignedTickets.Select(ta => ta.Ticket).ToList();
            var createdTickets = user.CreatedTickets.ToList();

            // Ticket counts by status
            var pending = assignedTickets.Count(t => t.Status == Status.Pending);
            var inProgress = assignedTickets.Count(t => t.Status == Status.InProgress);
            var completed = assignedTickets.Count(t => t.Status == Status.Completed || t.Status == Status.Closed);
            var rejected = assignedTickets.Count(t => t.Status == Status.Rejected);

            var completionRate = assignedTickets.Count > 0 ? (double)completed / assignedTickets.Count * 100 : 0;

            // Calculate average resolution days
            var completedTicketsList = assignedTickets.Where(t => t.Status == Status.Completed || t.Status == Status.Closed).ToList();
            var avgResolutionDays = 0.0;
            if (completedTicketsList.Any())
            {
                var resolutionTimes = completedTicketsList
                    .Select(t =>
                    {
                        var completedDate = t.Progresses
                            .Where(p => p.Note.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(p => p.Date)
                            .FirstOrDefault()?.Date ?? DateTime.UtcNow;
                        return (completedDate - t.CreatedAt).TotalDays;
                    })
                    .Where(days => days > 0)
                    .ToList();

                avgResolutionDays = resolutionTimes.Any() ? resolutionTimes.Average() : 0;
            }

            var currentWorkload = assignedTickets.Count(t => t.Status == Status.InProgress || t.Status == Status.Pending);
            var performanceRating = CalculatePerformanceRating(completionRate, currentWorkload);

            // Category Statistics
            var categoryStats = assignedTickets
                .Where(t => t.Category != null)
                .GroupBy(t => new { t.CategoryId, t.Category!.Name })
                .Select(g => new UserCategoryStatDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    TotalTickets = g.Count(),
                    CompletedTickets = g.Count(t => t.Status == Status.Completed || t.Status == Status.Closed),
                    CompletionRate = g.Count() > 0 ? Math.Round((double)g.Count(t => t.Status == Status.Completed || t.Status == Status.Closed) / g.Count() * 100, 2) : 0
                })
                .OrderByDescending(c => c.TotalTickets)
                .ToList();

            // CauseType Statistics (only completed tickets with cause type)
            var ticketsWithCauseType = completedTicketsList.Where(t => t.CauseType != null).ToList();
            var totalWithCause = ticketsWithCauseType.Count;
            var causeTypeStats = ticketsWithCauseType
                .GroupBy(t => new { t.CauseTypeId, t.CauseType!.Name })
                .Select(g => new UserCauseTypeStatDto
                {
                    CauseTypeId = g.Key.CauseTypeId!.Value,
                    CauseTypeName = g.Key.Name,
                    TotalTickets = g.Count(),
                    Percentage = totalWithCause > 0 ? Math.Round((double)g.Count() / totalWithCause * 100, 2) : 0
                })
                .OrderByDescending(c => c.TotalTickets)
                .ToList();

            // Monthly trend (last 6 months)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyTrend = new List<UserMonthlyTrendDto>();
            
            for (int i = 0; i < 6; i++)
            {
                var monthStart = sixMonthsAgo.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);
                var period = monthStart.ToString("yyyy-MM");

                var assignedInMonth = assignedTickets.Count(t => t.CreatedAt >= monthStart && t.CreatedAt < monthEnd);
                var completedInMonth = assignedTickets.Count(t => 
                    (t.Status == Status.Completed || t.Status == Status.Closed) &&
                    t.Progresses.Any(p => p.Date >= monthStart && p.Date < monthEnd && 
                                         p.Note.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase)));
                var createdInMonth = createdTickets.Count(t => t.CreatedAt >= monthStart && t.CreatedAt < monthEnd);

                monthlyTrend.Add(new UserMonthlyTrendDto
                {
                    Period = period,
                    TicketsAssigned = assignedInMonth,
                    TicketsCompleted = completedInMonth,
                    TicketsCreated = createdInMonth
                });
            }

            // Recent tickets (last 10)
            var recentTickets = assignedTickets
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new UserTicketSummaryDto
                {
                    TicketId = t.Id,
                    Title = t.Title,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    CategoryName = t.Category?.Name ?? "N/A",
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.Progresses
                        .Where(p => p.Note.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(p => p.Date)
                        .FirstOrDefault()?.Date
                })
                .ToList();

            var result = new UserDetailStatisticsDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                DepartmentName = user.Department?.Name ?? "N/A",
                Roles = roles.ToList(),
                TotalAssignedTickets = assignedTickets.Count,
                TotalCreatedTickets = createdTickets.Count,
                PendingTickets = pending,
                InProgressTickets = inProgress,
                CompletedTickets = completed,
                RejectedTickets = rejected,
                CompletionRate = Math.Round(completionRate, 2),
                AverageResolutionDays = Math.Round(avgResolutionDays, 2),
                PerformanceRating = performanceRating,
                CurrentWorkload = currentWorkload,
                CategoryStatistics = categoryStats,
                CauseTypeStatistics = causeTypeStats,
                MonthlyTrend = monthlyTrend,
                RecentTickets = recentTickets
            };

            return Result<UserDetailStatisticsDto>.IsSuccess(result);
        }
        catch (Exception ex)
        {
            return Result<UserDetailStatisticsDto>.Failure(new Error("UserDetailError", $"Failed to get user detail statistics: {ex.Message}"));
        }
    }

    public async Task<Result<List<EmployeePerformanceDto>>> GetAllUsersStatisticsAsync(int? departmentId = null)
    {
        try
        {
            var query = dbContext.Users
                .Include(u => u.Department)
                .Include(u => u.AssignedTickets)
                    .ThenInclude(ta => ta.Ticket)
                        .ThenInclude(t => t.Progresses)
                .Where(u => !departmentId.HasValue || u.DepartmentId == departmentId.Value);

            var users = await query.ToListAsync();
            var performances = new List<EmployeePerformanceDto>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var assignedTickets = user.AssignedTickets.Select(ta => ta.Ticket).ToList();

                var totalAssigned = assignedTickets.Count;
                var completed = assignedTickets.Count(t => t.Status == Status.Completed || t.Status == Status.Closed);

                var completionRate = totalAssigned > 0 ? (double)completed / totalAssigned * 100 : 0;

                // Calculate average resolution days
                var completedTickets = assignedTickets.Where(t => t.Status == Status.Completed || t.Status == Status.Closed).ToList();
                var avgResolutionDays = 0.0;
                if (completedTickets.Any())
                {
                    var resolutionTimes = completedTickets
                        .Select(t =>
                        {
                            var completedDate = t.Progresses
                                .Where(p => p.Note.Contains("hoàn thành", StringComparison.OrdinalIgnoreCase))
                                .OrderByDescending(p => p.Date)
                                .FirstOrDefault()?.Date ?? DateTime.UtcNow;
                            return (completedDate - t.CreatedAt).TotalDays;
                        })
                        .Where(days => days > 0)
                        .ToList();

                    avgResolutionDays = resolutionTimes.Any() ? resolutionTimes.Average() : 0;
                }

                var currentWorkload = assignedTickets.Count(t => t.Status == Status.InProgress || t.Status == Status.Pending);
                var performanceRating = CalculatePerformanceRating(completionRate, currentWorkload);

                performances.Add(new EmployeePerformanceDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    AssignedTickets = totalAssigned,
                    CompletedTickets = completed,
                    CompletionRate = Math.Round(completionRate, 2),
                    AverageResolutionDays = Math.Round(avgResolutionDays, 2),
                    CurrentWorkload = currentWorkload,
                    PerformanceRating = performanceRating,
                    Roles = roles.ToList()
                });
            }

            // Sort by fullname
            var sortedPerformances = performances.OrderBy(p => p.FullName).ToList();

            return Result<List<EmployeePerformanceDto>>.IsSuccess(sortedPerformances);
        }
        catch (Exception ex)
        {
            return Result<List<EmployeePerformanceDto>>.Failure(new Error("AllUsersStatisticsError", $"Failed to get all users statistics: {ex.Message}"));
        }
    }
}