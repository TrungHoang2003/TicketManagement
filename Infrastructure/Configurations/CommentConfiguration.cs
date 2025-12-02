using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        // Configure relationship with Ticket
        builder.HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with Creator (User)
        builder.HasOne(c => c.Creator)
            .WithMany()
            .HasForeignKey(c => c.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with Attachments
        builder.HasMany(c => c.Attachments)
            .WithOne()
            .HasForeignKey(a => a.EntityId)
            .HasPrincipalKey(c => c.Id);
    }
}
