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

        builder.Property(t => t.Id).ValueGeneratedOnAdd();
        
        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId);
        
        
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

        builder.HasMany(t => t.Progresses)
            .WithOne(p => p.Ticket)
            .HasForeignKey(p => p.TicketId);

        builder.HasOne(t => t.ImplementationPlan)
            .WithMany(ip => ip.Tickets)
            .HasForeignKey(t => t.ImplementationPlanId);
        
        builder.HasOne(t => t.CauseType)
            .WithMany(ct => ct.Tickets)
            .HasForeignKey(t => t.CauseTypeId);
        
        builder.Property(t => t.Status).HasConversion<string>();
        builder.Property(t => t.Priority).HasConversion<string>();
    }
}