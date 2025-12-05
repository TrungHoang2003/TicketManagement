namespace Application.DTOs;

// Main Admin Dashboard DTO
public class AdminDashboardDto
{
    public required SystemOverviewDto SystemOverview { get; set; }
    public required List<EmployeePerformanceDto> TopPerformers { get; set; }
    public required List<EmployeePerformanceDto> UnderPerformers { get; set; }
    public required List<DepartmentPerformanceDto> DepartmentPerformance { get; set; }
    public required List<TrendAnalysisDto> TrendAnalysis { get; set; }
    public required List<RealtimeAlertDto> RealtimeAlerts { get; set; }
}

// System Overview Statistics
public class SystemOverviewDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalTickets { get; set; }
    public int TotalPendingTickets { get; set; }
    public int TotalInProgressTickets { get; set; }
    public int TotalCompletedTickets { get; set; }
    public int TotalRejectedTickets { get; set; }
    public int TotalOverdueTickets { get; set; } // Số ticket quá hạn
    public int CompletedOnTime { get; set; } // Hoàn thành đúng hạn
    public int CompletedLate { get; set; } // Hoàn thành trễ hạn
    public double OverallCompletionRate { get; set; }
    public double OnTimeRate { get; set; } // Tỉ lệ hoàn thành đúng hạn
    public double AverageResolutionDays { get; set; }
    public required List<CategoryStatisticDto> CategoryStatistics { get; set; }
    public required List<CauseTypeStatisticDto> CauseTypeStatistics { get; set; }
    public DateTime LastUpdated { get; set; }
}

// Employee Performance Metrics
public class EmployeePerformanceDto
{
    public int UserId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string DepartmentName { get; set; }
    public int AssignedTickets { get; set; }
    public int ManagedTickets { get; set; } // Số ticket đang quản lý (là Head)
    public int CompletedTickets { get; set; }
    public int OverdueTickets { get; set; } // Số ticket đang quá hạn
    public int CompletedOnTime { get; set; } // Hoàn thành đúng hạn
    public int CompletedLate { get; set; } // Hoàn thành trễ hạn
    public double CompletionRate { get; set; }
    public double OnTimeRate { get; set; } // Tỉ lệ hoàn thành đúng hạn
    public double AverageResolutionDays { get; set; }
    public int CurrentWorkload { get; set; }
    public required string PerformanceRating { get; set; } // Excellent, Good, Average, Poor
    public required List<string> Roles { get; set; }
}

// Department Performance Metrics  
public class DepartmentPerformanceDto
{
    public int DepartmentId { get; set; }
    public required string DepartmentName { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalTickets { get; set; }
    public int PendingTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int OverdueTickets { get; set; } // Số ticket đang quá hạn
    public double CompletionRate { get; set; }
    public double OnTimeRate { get; set; } // Tỉ lệ hoàn thành đúng hạn
    public double AverageResolutionDays { get; set; }
    public double WorkloadDistribution { get; set; }
    public required string PerformanceRating { get; set; }
    public required List<CategoryStatDto> TopCategories { get; set; }
}

// Trend Analysis
public class TrendAnalysisDto
{
    public required string Period { get; set; } // e.g., "2024-12", "Week 48", "2024-12-05"
    public int TicketsCreated { get; set; }
    public int TicketsCompleted { get; set; }
    public int TicketsInProgress { get; set; }
    public double CompletionRate { get; set; }
    public double AverageResolutionTime { get; set; }
    public int ActiveUsers { get; set; }
}

// Realtime Alerts
public class RealtimeAlertDto
{
    public required string Type { get; set; } // SLA_VIOLATION, OVERDUE_TICKET, HIGH_WORKLOAD, etc.
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Severity { get; set; } // Critical, Warning, Info
    public DateTime CreatedAt { get; set; }
    public int? TicketId { get; set; }
    public int? UserId { get; set; }
    public int? DepartmentId { get; set; }
    public required string ActionUrl { get; set; }
}

// Workload Analysis
public class WorkloadAnalysisDto
{
    public int UserId { get; set; }
    public required string FullName { get; set; }
    public required string DepartmentName { get; set; }
    public int CurrentTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    public int OverdueTickets { get; set; } // Số ticket đang quá hạn
    public double WorkloadScore { get; set; } // Calculated based on various factors
    public required string WorkloadLevel { get; set; } // Light, Normal, Heavy, Overloaded
}

// Time-based Performance
public class TimeBasedPerformanceDto
{
    public required string TimeLabel { get; set; }
    public int TicketsCreated { get; set; }
    public int TicketsResolved { get; set; }
    public double AverageResolutionTime { get; set; }
    public double SlaCompliance { get; set; }
}

// Category Performance across system
public class CategoryPerformanceDto
{
    public int CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public required string DepartmentName { get; set; }
    public int TotalTickets { get; set; }
    public int CompletedTickets { get; set; }
    public double AverageResolutionDays { get; set; }
    public double CompletionRate { get; set; }
    public required string ComplexityLevel { get; set; } // Simple, Medium, Complex
}

// Category Statistics (for System Overview)
public class CategoryStatisticDto
{
    public int CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public int TotalTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int PendingTickets { get; set; }
    public double CompletionRate { get; set; }
}

// CauseType Statistics (for System Overview)
public class CauseTypeStatisticDto
{
    public int CauseTypeId { get; set; }
    public required string CauseTypeName { get; set; }
    public int TotalTickets { get; set; }
    public double Percentage { get; set; }
}

// User Detail Statistics - Thống kê chi tiết theo user
public class UserDetailStatisticsDto
{
    public int UserId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string DepartmentName { get; set; }
    public required List<string> Roles { get; set; }
    
    // Ticket Overview
    public int TotalAssignedTickets { get; set; }
    public int TotalManagedTickets { get; set; } // Tickets đang quản lý (là Head)
    public int TotalCreatedTickets { get; set; }
    public int PendingTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int RejectedTickets { get; set; }
    public double CompletionRate { get; set; }
    public double AverageResolutionDays { get; set; }
    
    // Overdue statistics
    public int OverdueTickets { get; set; }
    public double OnTimeRate { get; set; }
    
    // Performance metrics
    public required string PerformanceRating { get; set; }
    public int CurrentWorkload { get; set; }
    
    // Statistics by Category
    public required List<UserCategoryStatDto> CategoryStatistics { get; set; }
    
    // Statistics by CauseType (for completed tickets)
    public required List<UserCauseTypeStatDto> CauseTypeStatistics { get; set; }
    
    // Monthly trend (last 6 months)
    public required List<UserMonthlyTrendDto> MonthlyTrend { get; set; }
    
    // Recent tickets
    public required List<UserTicketSummaryDto> RecentTickets { get; set; }
}

// User Category Statistics
public class UserCategoryStatDto
{
    public int CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public int TotalTickets { get; set; }
    public int CompletedTickets { get; set; }
    public double CompletionRate { get; set; }
}

// User CauseType Statistics
public class UserCauseTypeStatDto
{
    public int CauseTypeId { get; set; }
    public required string CauseTypeName { get; set; }
    public int TotalTickets { get; set; }
    public double Percentage { get; set; }
}

// User Monthly Trend
public class UserMonthlyTrendDto
{
    public required string Period { get; set; }
    public int TicketsAssigned { get; set; }
    public int TicketsCompleted { get; set; }
    public int TicketsCreated { get; set; }
}

// User Ticket Summary (for recent tickets list)
public class UserTicketSummaryDto
{
    public int TicketId { get; set; }
    public required string Title { get; set; }
    public required string Status { get; set; }
    public required string Priority { get; set; }
    public required string CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}