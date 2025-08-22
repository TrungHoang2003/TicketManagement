using BuildingBlocks.Commons;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public interface IUserRepository 
{
    Task<User> FindByIdAsync(int id);
    Task<User> FindByNameAsync(string userName);
    Task<IList<string>> GetRolesAsync(User user);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<IdentityResult> CreateAsync(User user, string? password = null);
    Task<User?> FindByEmailAsync(string email);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<User> GetHeadOfDepartment(string departmentName);
}

public class UserRepository(UserManager<User> userManager, ILogger<UserRepository> logger) :  IUserRepository
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

    public async Task<User?> FindByEmailAsync(string email)
    {
        var result = await userManager.FindByEmailAsync(email);
        return result;
    }

    public async Task<IdentityResult> CreateAsync(User user, string? password = null)
    {
        return password == null
            ? await userManager.CreateAsync(user)
            : await userManager.CreateAsync(user, password);
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

    public async Task<User> GetHeadOfDepartment(string departmentName)
    {
        var role = departmentName switch
        {
            "AD" => "Head Of AD",
            "QA" => "Head Of QA",
            "IT" => "Head Of IT",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var users = await userManager.GetUsersInRoleAsync(role);
        return users.FirstOrDefault() ?? throw new BusinessException($"No head found for department: {departmentName}");
    }
}
