using System.Collections.Immutable;
using BuildingBlocks.Commons;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public interface IUserRepository
{
    Task<IdentityResult> UpdateAsync(User user);
    Task<User> FindByIdAsync(int id);
    Task<User> FindByNameAsync(string userName);
    Task<IList<string>> GetRolesAsync(User user);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<IdentityResult> CreateAsync(User user, string? password = null);
    Task<User> FindByEmailAsync(string email);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<User> GetHeadOfDepartment(int departmentId);
    Task<User> GetAdmin();
}

public class UserRepository(UserManager<User> userManager, ILogger<UserRepository> logger, AppDbContext dbContext) :  IUserRepository
{
    public async Task<User> FindByIdAsync(int id)
    {
        var result =  await userManager.FindByIdAsync(id.ToString());
        if (result == null) throw new BusinessException($"User Not found with Id = {id}");
        return result;
    }

    public async Task<User> FindByNameAsync(string userName)
    {
        var result = await userManager.FindByNameAsync(userName);
        if (result == null) throw new BusinessException($"User {userName} not found");
        return result;
    }

    public async Task<User> FindByEmailAsync(string email)
    {
        var result = await userManager.FindByEmailAsync(email);
        if (result == null) throw new BusinessException($"User {email} not found");
        return result;
    }

    public async Task<IdentityResult> CreateAsync(User user, string? password = null)
    {
        return password == null
            ? await userManager.CreateAsync(user)
            : await userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> UpdateAsync(User user)
    {
        return await userManager.UpdateAsync(user);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return await userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        try
        {
            return await userManager.GetRolesAsync(user);
        }
        catch (Exception e)
        {
            logger.LogError("Error while getting Roles: {e.Message}", e.Message);
            throw;
        }
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        return await userManager.AddToRoleAsync(user, role);
    }

    public async Task<User> GetHeadOfDepartment(int departmentId)
    {
        var users = await userManager.GetUsersInRoleAsync("Head");
        var head = users.FirstOrDefault(u => u.DepartmentId == departmentId);
        return head ?? throw new BusinessException($"No head found for department: {departmentId}");
    }

    public async Task<User> GetAdmin()
    {
        var result = await userManager.GetUsersInRoleAsync("admin");
        return result.First();
    }
}
