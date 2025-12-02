using System.Security.Claims;
using Application.DTOs;
using Application.Erros;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Commons;
using Infrastructure.Repositories;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface IUserService
{
   Task<Result<UserLoginResponse>> Login(UserLoginRequest userLoginDto);
   Task<Result> Create(CreateUserRequest createUserDto);
   Task<Result> Update(UpdateUserRequest updateUserRequest);
   Task<Result<string>> UpdateAvatar(UpdateAvatarRequest request);
   Task<Result<List<UsersByDepartmentDto>>> GetByDepartment(int departmentId);
   Task<Result<UserDto>> GetById(int id);
   Task<Result<List<UserDto>>> GetAll();
   int GetLoginUserId();
}

public class UserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepo, IJwtService jwtService,
   IRedisService redisService, IDepartmentRepository departmentRepo, AppDbContext dbContext, UserManager<User> userManager, IMapper mapper, ICloudinaryService cloudinaryService) : IUserService
{
   public async Task<Result<string>> UpdateAvatar(UpdateAvatarRequest request)
   {
      var user = await userRepo.FindByIdAsync(request.UserId);
      if (user == null) return Result<string>.Failure(UserErrors.UserNotFound);

      try
      {
         var avatarUrl = await cloudinaryService.UploadFile(request.Base64Image, $"avatar_{user.Id}");
         user.AvatarUrl = avatarUrl;
         
         var result = await userRepo.UpdateAsync(user);
         if (!result.Succeeded)
         {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result<string>.Failure(new Error("UpdateAvatar.Failed", string.Join(",", errors)));
         }

         return Result<string>.IsSuccess(avatarUrl);
      }
      catch (Exception ex)
      {
         return Result<string>.Failure(new Error("UpdateAvatar.UploadFailed", ex.Message));
      }
   }

   public async Task<Result> Update(UpdateUserRequest updateUserRequest)
   {
      var existingUser = await userRepo.FindByIdAsync(updateUserRequest.Id);

      if(updateUserRequest.DepartmentId.HasValue) existingUser.DepartmentId = updateUserRequest.DepartmentId.Value;
      if (updateUserRequest.FullName != null) existingUser.FullName = updateUserRequest.FullName;
      if (updateUserRequest.Username != null) existingUser.UserName = updateUserRequest.Username;
      
      // Handle roles update - replace all roles with new ones
      if (updateUserRequest.Roles != null)
      {
         // Get current roles
         var currentRoles = await userManager.GetRolesAsync(existingUser);
         
         // Find roles to remove and roles to add
         var rolesToRemove = currentRoles.Except(updateUserRequest.Roles).ToList();
         var rolesToAdd = updateUserRequest.Roles.Except(currentRoles).ToList();
         
         // Remove roles that are no longer needed
         if (rolesToRemove.Any())
         {
            var removeResult = await userManager.RemoveFromRolesAsync(existingUser, rolesToRemove);
            if (!removeResult.Succeeded)
            {
               var removeErrors = removeResult.Errors.Select(e => e.Description).ToList();
               return Result.Failure(new Error("Remove Roles Failed: ", string.Join(",", removeErrors)));
            }
         }
         
         // Add new roles
         if (rolesToAdd.Any())
         {
            var addResult = await userManager.AddToRolesAsync(existingUser, rolesToAdd);
            if (!addResult.Succeeded)
            {
               var addErrors = addResult.Errors.Select(e => e.Description).ToList();
               return Result.Failure(new Error("Add Roles Failed: ", string.Join(",", addErrors)));
            }
         }
      }

      var result = await userRepo.UpdateAsync(existingUser);
      if (result.Succeeded) return Result.IsSuccess();
      
      var updateErrors = result.Errors.Select(e => e.Description).ToList();
      return Result.Failure(new Error("Update User Failed: ", string.Join(",", updateErrors)));
   }

   public async Task<Result<List<UsersByDepartmentDto>>> GetByDepartment(int departmentId)
   {
      var users = await dbContext.Users.Where(u=>u.DepartmentId == departmentId)
         .Select(u => new UsersByDepartmentDto { Id = u.Id, FullName = u.FullName})
         .ToListAsync();

      return users;
   }

    public async Task<Result<UserDto>> GetById(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null) return UserErrors.UserNotFound;
        
        var roles = await userManager.GetRolesAsync(user);
        
        var userDto = mapper.Map<UserDto>(user);
        userDto.Roles = roles;
        return userDto;
    }

    public async Task<Result<List<UserDto>>> GetAll()
    {
       
       var users = await dbContext.Users.AsQueryable().ProjectTo<UserDto>(mapper.ConfigurationProvider).ToListAsync();

       foreach (var user in users)
       {
          var roles = await userRepo.GetRolesAsync(await userRepo.FindByIdAsync(user.Id));
            user.Roles = roles;
       }

       return users;
    }


    public int GetLoginUserId()
   {
      var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      _ = int.TryParse(userIdClaim, out var userId);
      return userId;
   }

   public async Task<Result<UserLoginResponse>> Login(UserLoginRequest userLoginDto)
   {
      var user = await userRepo.FindByEmailAsync(userLoginDto.Email);

      var isPasswordValid = await userRepo.CheckPasswordAsync(user, userLoginDto.Password);
      if (!isPasswordValid)
         return AuthenErrors.WrongPassWord;

      var roles = await userRepo.GetRolesAsync(user);
      var accessToken = jwtService.GenerateJwtToken(user, roles);
      var refreshToken = jwtService.GenerateRefreshToken();

      var refreshKey = $"refreshToken:{user.Id}";
      var accessKey = $"accessToken:{user.Id}";

      await redisService.SetValue(refreshKey, refreshToken, TimeSpan.FromDays(jwtService.GetRefreshTokenValidity()));
      await redisService.SetValue(accessKey, accessToken, TimeSpan.FromMinutes(jwtService.GetAccessTokenValidity()));

      return Result<UserLoginResponse>.IsSuccess(new UserLoginResponse
      {
         AccessToken = accessToken,
         RefreshToken = refreshToken,
         FullName = user.FullName,
         Email = user.Email!,
      });
   }

   public async Task<Result> Create(CreateUserRequest dto)
   {
      var department = await departmentRepo.GetByIdAsync(dto.DepartmentId);
      
      var user = new User
      {
         FullName = dto.FullName,
         UserName = dto.Email,
         Email = dto.Email,
         Department = department,
         DepartmentId = department.Id
      };

      var result = await userRepo.CreateAsync(user);
      if (!result.Succeeded)
      {
         var errors = result.Errors.Select(e => e.Description).ToList();
         return Result.Failure(new Error("Register.Failed", string.Join(",", errors)));
      }

      var roleResult = await userRepo.AddToRolesAsync(user, dto.Roles);
      if (!roleResult.Succeeded)
      {
         var roleErrors = roleResult.Errors.Select(e => e.Description).ToList();
         return Result.Failure(new Error("Add Roles Failed", string.Join(",", roleErrors)));
      }
         
      return Result.IsSuccess();
   }
}