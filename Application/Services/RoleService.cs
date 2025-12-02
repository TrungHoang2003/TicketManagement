using Application.DTOs;
using BuildingBlocks.Commons;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface IRoleService
{
    Task<Result> Create(CreateRoleDto createRoleDto);
    Task<Result> Update(UpdateRoleDto updateRoleDto);
    Task<Result> Delete(int roleId);
    Task<Result<List<RoleDto>>> GetAll();
    Task<Result<RoleDto>> GetById(int roleId);
}

public class RoleService(RoleManager<IdentityRole<int>> roleManager) : IRoleService
{
    public async Task<Result> Create(CreateRoleDto createRoleDto)
    {
        var role = new IdentityRole<int>
        {
            Name = createRoleDto.Name,
            NormalizedName = createRoleDto.Name.ToUpper()
        };

        var result = await roleManager.CreateAsync(role);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error("CreateRoleFailed", errors));
        }

        return Result.IsSuccess();
    }

    public async Task<Result> Update(UpdateRoleDto updateRoleDto)
    {
        var role = await roleManager.FindByIdAsync(updateRoleDto.Id.ToString());
        
        if (role == null)
        {
            return Result.Failure(new Error("RoleNotFound", "Role not found"));
        }

        role.Name = updateRoleDto.Name;
        role.NormalizedName = updateRoleDto.Name.ToUpper();

        var result = await roleManager.UpdateAsync(role);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error("UpdateRoleFailed", errors));
        }

        return Result.IsSuccess();
    }

    public async Task<Result> Delete(int roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        
        if (role == null)
        {
            return Result.Failure(new Error("RoleNotFound", "Role not found"));
        }

        var result = await roleManager.DeleteAsync(role);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error("DeleteRoleFailed", errors));
        }

        return Result.IsSuccess();
    }

    public async Task<Result<List<RoleDto>>> GetAll()
    {
        var roles = await roleManager.Roles
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                NormalizedName = r.NormalizedName,
                ConcurrencyStamp = r.ConcurrencyStamp
            })
            .ToListAsync();

        return Result<List<RoleDto>>.IsSuccess(roles);
    }

    public async Task<Result<RoleDto>> GetById(int roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        
        if (role == null)
        {
            return new Error("RoleNotFound", "Role not found");
        }

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            NormalizedName = role.NormalizedName,
            ConcurrencyStamp = role.ConcurrencyStamp
        };

        return Result<RoleDto>.IsSuccess(roleDto);
    }
}
