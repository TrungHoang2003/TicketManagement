using Domain.Entities;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

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
            new IdentityRole<int> { Id = 5, Name = "Head Of QA", NormalizedName = "HEAD OF QA" },
            new IdentityRole<int> { Id = 6, Name = "Head", NormalizedName = "HEAD" }
        );
        
        builder.Entity<User>().HasData(
            new User { Id = 2, FullName= "Nguyễn Quang Hà", NormalizedUserName = "NGUYEN QUANG HA", Email = "quangha27103@gmail.com", NormalizedEmail = "QUANGHA27103@GMAIL.COM", DepartmentId = 3, EmailConfirmed = true },
            new User { Id = 3, FullName= "admin", NormalizedUserName = "ADMIN", Email = "21a10010397@students.hou.edu.vn", NormalizedEmail = "21A10010397@STUDENTS.HOU.EDU.VN", DepartmentId = 3, EmailConfirmed = true },
            new User { Id = 4, FullName = "Lê Văn Thiện", NormalizedUserName = "LE VAN THIEN", Email = "levanthien332003@gmail.com", NormalizedEmail = "LEVANTHIEN332003@GMAIL.COM", DepartmentId = 1, EmailConfirmed = true },
            new User { Id = 5, FullName= "Hoàng Việt Trung", NormalizedUserName = "HOANG VIET TRUNG", Email = "trunghoang220703@gmail.com", NormalizedEmail = "TRUNGHOANG220703@GMAIL.COM", DepartmentId = 2, EmailConfirmed = true },
            new User { Id = 6, FullName= "Nguyễn Minh Sơn", NormalizedUserName = "NGUYEN MINH SON", Email = "minhson6a1@gmail.com", NormalizedEmail = "MINHSON6A1@GMAIL.COM", DepartmentId = 2, EmailConfirmed = true }
        );

        builder.Entity<IdentityUserRole<int>>().HasData(
            new IdentityUserRole<int> { UserId = 2, RoleId = 5 }, // Head Of QA
            new IdentityUserRole<int> { UserId = 3, RoleId = 3 }, // Admin
            new IdentityUserRole<int> { UserId = 4, RoleId = 2 }, // Head Of AD
            new IdentityUserRole<int> { UserId = 5, RoleId = 4 }, // Head Of IT
            
            new IdentityUserRole<int> { UserId = 2, RoleId = 6 }, // Head
            new IdentityUserRole<int> { UserId = 3, RoleId = 6 }, // Head
            new IdentityUserRole<int> { UserId = 4, RoleId = 6 }, // Head 
            new IdentityUserRole<int> { UserId = 5, RoleId = 6 }, // Head
            
            new IdentityUserRole<int> { UserId = 6, RoleId = 1 } // Employee
        );
        
        builder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "AD" },
            new Department { Id = 2, Name = "IT" },
            new Department { Id = 3, Name = "QA" });

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Sửa chữa thiết bị văn phòng", DepartmentId = 1},
            new Category { Id = 2, Name = "Vấn đề về điện, nước, điều hòa",DepartmentId = 1},
            new Category { Id = 3, Name = "Bảo trì cơ sở hạ tầng", DepartmentId = 1 },
            new Category { Id = 4, Name = "Vấn đề về an ninh, bảo vệ",DepartmentId = 1 },
            new Category { Id = 5, Name = "Khiếu nại dịch vụ", DepartmentId = 3},
            new Category { Id = 6, Name = "Yêu cầu tư vấn sản phẩm",DepartmentId = 3 },
            new Category { Id = 7, Name = "Phản hồi chất lượng", DepartmentId = 3 },
            new Category { Id = 8, Name = "Giải quyết tranh chấp",DepartmentId = 3 },
            new Category { Id = 9, Name = "Lỗi phần mềm, ứng dụng", DepartmentId = 3 },
            new Category { Id = 10, Name = "Vấn đề mạng, kết nối",DepartmentId = 3 },
            new Category { Id = 11, Name = "Cài đặt, cấu hình thiết bị IT",DepartmentId = 2 });

        builder.Entity<Attachment>().Property(a => a.EntityType)
            .HasConversion<string>();

        builder.ApplyConfiguration(new TicketConfiguration());
    }
}