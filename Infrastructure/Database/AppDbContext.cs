using Domain.Entities;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Ticket>().ToTable("Tickets");
        builder.Entity<Comment>().ToTable("Comments");
        builder.Entity<Category>().ToTable("Categories");
        builder.Entity<Department>().ToTable("Departments");
        builder.Entity<History>().ToTable("Histories");
        builder.Entity<Attachment>().ToTable("Attachments");
        builder.Entity<CauseType>().ToTable("CauseTypes");

        builder.Entity<User>().ToTable("Users");
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        builder.Entity<IdentityRole<int>>().HasData(
            new IdentityRole<int> { Id = 1, Name = "Employee", NormalizedName = "EMPLOYEE" },
            new IdentityRole<int> { Id = 2, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<int> { Id = 3, Name = "Head Of IT", NormalizedName = "HEAD OF IT" },
            new IdentityRole<int> { Id = 4, Name = "Head Of AD", NormalizedName = "HEAD OF AD" },
            new IdentityRole<int> { Id = 5, Name = "Head Of Sales", NormalizedName = "HEAD OF SALES" },
            new IdentityRole<int> { Id = 6, Name = "Head Of HR", NormalizedName = "HEAD OF HR" },
            new IdentityRole<int> { Id = 7, Name = "Head Of Finance", NormalizedName = "HEAD OF FINANCE" },
            new IdentityRole<int> { Id = 8, Name = "Head", NormalizedName = "HEAD" }
        );

        builder.Entity<Attachment>().Property(a => a.EntityType)
            .HasConversion<string>();

        builder.ApplyConfiguration(new TicketConfiguration());
        builder.ApplyConfiguration(new TicketAssigneeConfiguration());
    }
}