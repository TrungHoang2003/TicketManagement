using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TicketAssigneeConfiguration : IEntityTypeConfiguration<TicketAssignee>
{
    public void Configure(EntityTypeBuilder<TicketAssignee> builder)
    {
        builder.ToTable("TicketAssignees");

        builder.HasKey(a => new { a.TicketId, a.AssigneeId });

        builder.HasOne(a => a.Ticket)
            .WithMany(t => t.Assignees)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Assignee)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(a => a.AssigneeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
public class TicketHeadConfiguration: IEntityTypeConfiguration<TicketHead>
{
    public void Configure(EntityTypeBuilder<TicketHead> builder)
    {
        builder.ToTable("TicketHeads");

        builder.HasKey(a => new { a.TicketId, a.HeadId});

        builder.HasOne(a => a.Ticket)
            .WithMany(t => t.Heads)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Head)
            .WithMany(u => u.FollowingTicket)
            .HasForeignKey(a => a.HeadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
