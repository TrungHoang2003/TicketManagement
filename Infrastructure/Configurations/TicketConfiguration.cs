using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TicketConfiguration:IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        
        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId);
        
        // Cấu hình relationship với Assignee
        builder.HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Cấu hình relationship với Creator
        builder.HasOne(t => t.Creator)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Cấu hình relationship với HeadDepartment
        builder.HasOne(t => t.HeadOfDepartment)
            .WithMany(u => u.FollowingTickets)
            .HasForeignKey(t => t.HeadDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(t => t.Comments)
            .WithOne(c=>c.Ticket)
            .HasForeignKey(h=>h.TicketId);
        
        builder.Property(t => t.Status).HasConversion<string>();
        builder.Property(t => t.Priority).HasConversion<string>();
    }
}