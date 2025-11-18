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
   Task<Result<List<UsersByDepartmentDto>>> GetByDepartment(int departmentId);
   Task<Result<UserDto>> GetById(int id);
   Task<Result<List<UserDto>>> GetAll();
   int GetLoginUserId();
}

public class UserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepo, IJwtService jwtService,
   IRedisService redisService, IDepartmentRepository departmentRepo, AppDbContext dbContext, UserManager<User> userManager, IMapper mapper) : IUserService
{
   public async Task<Result> Update(UpdateUserRequest updateUserRequest)
   {
      var existingUser = await userRepo.FindByIdAsync(updateUserRequest.Id);

      if(updateUserRequest.DepartmentId.HasValue) existingUser.DepartmentId = updateUserRequest.DepartmentId.Value;
      if (updateUserRequest.FullName != null) existingUser.FullName = updateUserRequest.FullName;
      if (updateUserRequest.Username != null) existingUser.UserName = updateUserRequest.Username;
      if (updateUserRequest.Roles != null)
         await userRepo.AddToRolesAsync(existingUser, updateUserRequest.Roles);

      var result = await userRepo.UpdateAsync(existingUser);
      if (result.Succeeded) return Result.IsSuccess();
      
      var errors = result.Errors.Select(e => e.Description).ToList();
      return Result.Failure(new Error("Update User Failed: ", string.Join(",", errors)));
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
       return await dbContext.Users.AsQueryable().ProjectTo<UserDto>(mapper.ConfigurationProvider).ToListAsync();
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

      await userRepo.AddToRolesAsync(user, dto.Roles);
         
      return Result.IsSuccess();
   }
}