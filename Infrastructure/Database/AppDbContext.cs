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
        builder.Entity<Project>().ToTable("Projects");
        builder.Entity<CauseType>().ToTable("CauseTypes");
        builder.Entity<ImplementationPlan>().ToTable("ImplementationPlans");

        builder.Entity<User>().ToTable("Users");
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        builder.Entity<IdentityRole<int>>().HasData(
            new IdentityRole<int> { Id = 1, Name = "Employee", NormalizedName = "EMPLOYEE" },
            new IdentityRole<int> { Id = 3, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<int> { Id = 4, Name = "Head Of IT", NormalizedName = "HEAD OF IT" },
            new IdentityRole<int> { Id = 2, Name = "Head Of AD", NormalizedName = "HEAD OF AD" },
        new IdentityRole<int> { Id = 5, Name = "Head Of QA", NormalizedName = "HEAD OF QA" }
        );

        builder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "AD" },
            new Department { Id = 2, Name = "IT" },
            new Department { Id = 3, Name = "QA" });

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Sửa chữa thiết bị văn phòng", Department = DepartmentEnum.Ad },
            new Category { Id = 2, Name = "Vấn đề về điện, nước, điều hòa", Department = DepartmentEnum.Ad },
            new Category { Id = 3, Name = "Bảo trì cơ sở hạ tầng", Department = DepartmentEnum.Ad },
            new Category { Id = 4, Name = "Vấn đề về an ninh, bảo vệ", Department = DepartmentEnum.Ad },
            new Category { Id = 5, Name = "Khiếu nại dịch vụ", Department = DepartmentEnum.Qa },
            new Category { Id = 6, Name = "Yêu cầu tư vấn sản phẩm", Department = DepartmentEnum.Qa },
            new Category { Id = 7, Name = "Phản hồi chất lượng", Department = DepartmentEnum.Qa },
            new Category { Id = 8, Name = "Giải quyết tranh chấp", Department = DepartmentEnum.Qa },
            new Category { Id = 9, Name = "Lỗi phần mềm, ứng dụng", Department = DepartmentEnum.Qa },
            new Category { Id = 10, Name = "Vấn đề mạng, kết nối", Department = DepartmentEnum.Qa },
            new Category { Id = 11, Name = "Cài đặt, cấu hình thiết bị IT", Department = DepartmentEnum.It });

        builder.Entity<Attachment>().Property(a => a.EntityType)
            .HasConversion<string>();

        builder.ApplyConfiguration(new TicketConfiguration());
    }
}