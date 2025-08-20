using System.Security.Claims;
using Application.DTOs;
using Application.Erros;
using Application.Shared;
using BuildingBlocks.Commons;
using Infrastructure.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Shared.Services;

namespace Application.Services;

public interface IUserService
{
   Task<Result<UserLoginResponse>> Login(UserLoginDto userLoginDto);
   Task<Result> Create(CreateUserDto createUserDto);
   Task<Result<int>> GetUserId();
}

public class UserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepo, IJwtService jwtService,
   IRedisService redisService, IDepartmentRepository departmentRepo) : IUserService
{
   public Task<Result<int>> GetUserId()
   {
      var userIdClaim = httpContextAccessor.
         HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      return Task.FromResult(!int.TryParse(userIdClaim, out var userId) ?
         ParseError.UserIdParseError : Result<int>.IsSuccess(userId));
   }

   public async Task<Result<UserLoginResponse>> Login(UserLoginDto userLoginDto)
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

   public async Task<Result> Create(CreateUserDto dto)
   {
      var department = await departmentRepo.GetByIdAsync(dto.DepartmentId);
      
      var user = new User
      {
         FullName = dto.FullName,
         UserName = dto.Email,
         Email = dto.Email,
         Department = department
      };

      var result = await userRepo.CreateAsync(user, dto.Password);
      if (!result.Succeeded)
      {
         var errors = result.Errors.Select(e => e.Description).ToList();
         return Result.Failure(new Error("Register.Failed", string.Join(",", errors)));
      }

      await userRepo.AddToRoleAsync(user, dto.Role);
      return Result.IsSuccess();
   }
}