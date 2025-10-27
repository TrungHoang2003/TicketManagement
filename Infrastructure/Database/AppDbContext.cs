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
            new IdentityRole<int> { Id = 5, Name = "Head Of QA", NormalizedName = "HEAD OF QA" },
            new IdentityRole<int> { Id = 6, Name = "Head", NormalizedName = "HEAD" }
        );

        builder.Entity<User>().HasData(
            new User
            {
                Id = 2,
                UserName = "ha123",
                FullName = "Nguyễn Quang Hà",
                NormalizedUserName = "HA123",
                Email = "quangha27103@gmail.com",
                NormalizedEmail = "QUANGHA27103@GMAIL.COM",
                DepartmentId = 3,
                EmailConfirmed = true,
                SecurityStamp = "5b1d9f4e-3a2c-4d7f-9f0a-1a2b3c4d5e6f",
                ConcurrencyStamp = "d3e9a1c7-4b2f-4e0a-97c9-79a58a7dbe12",
                PasswordHash = null
            },
            new User
            {
                Id = 3,
                UserName = "admin",
                FullName = "Admin",
                NormalizedUserName = "ADMIN",
                Email = "21a10010397@students.hou.edu.vn",
                NormalizedEmail = "21A10010397@STUDENTS.HOU.EDU.VN",
                DepartmentId = 3,
                EmailConfirmed = true,
                SecurityStamp = "d2f8a7c1-6b3e-4f8a-9c2d-7e8f9a0b1c2d",
                ConcurrencyStamp = "b6e5d2a3-8f2b-43a1-9053-1a9f6e4c1a21",
                PasswordHash = null
            },
            new User
            {
                Id = 4,
                UserName = "thien123",
                FullName = "Lê Văn Thiện",
                NormalizedUserName = "THIEN123",
                Email = "levanthien332003@gmail.com",
                NormalizedEmail = "LEVANTHIEN332003@GMAIL.COM",
                DepartmentId = 1,
                EmailConfirmed = true,
                SecurityStamp = "a7c9e2f4-1b3d-4a6f-8c9d-0e1f2a3b4c5d",
                ConcurrencyStamp = "f2a1b7c5-8c9d-4a1b-9e3f-7c8b6a4d2e1f",
                PasswordHash = null
            },
            new User
            {
                Id = 5,
                UserName = "trung123",
                FullName = "Hoàng Việt Trung",
                NormalizedUserName = "TRUNG123",
                Email = "trunghoang220703@gmail.com",
                NormalizedEmail = "TRUNGHOANG220703@GMAIL.COM",
                DepartmentId = 2,
                EmailConfirmed = true,
                SecurityStamp = "c3e4b5a6-7d8f-4a1b-9c0d-2e3f4a5b6c7d",
                ConcurrencyStamp = "a9c8b7d6-1e2f-4a3b-9d0e-5f6a7c8b9e0f",
                PasswordHash = null
            },
            new User
            {
                Id = 6,
                UserName = "son123",
                FullName = "Nguyễn Minh Sơn",
                NormalizedUserName = "SON123",
                Email = "minhson6a1@gmail.com",
                NormalizedEmail = "MINHSON6A1@GMAIL.COM",
                DepartmentId = 2,
                EmailConfirmed = true,
                SecurityStamp = "9f8e7d6c-5b4a-3c2d-1e0f-9a8b7c6d5e4f",
                ConcurrencyStamp = "d8f5a2b1-3c9e-47a2-86f0-1b2d3e4f5a6b",
                PasswordHash = null
            }
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