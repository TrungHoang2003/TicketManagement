using Application.DTOs;
using BuildingBlocks.Commons;
using Infrastructure.Repositories;
using Application.Erros;
using Domain.Entities;

namespace Application.Services;

public interface IImplementationPlanService
{
    Task<Result> Create(CreateImplementationPlanRequest createImplementationPlanRequest);
    Task<Result> Update(UpdateImplementationPlanDto updateImplementationPlanDto);
    Task<Result> Delete(int implementationPlanId);
    Task<Result<List<ImplementationPlanDto>>> GetAll();
    Task<Result<ImplementationPlanDto>> GetById(int implementationPlanId);
};

public class ImplementationPlanService(IUnitOfWork unitOfWork) : IImplementationPlanService
{
    public async Task<Result> Create(CreateImplementationPlanRequest createImplementationPlanRequest)
    {
        var implementationPlan = new ImplementationPlan
        {
            Name = createImplementationPlanRequest.Name,
            Description = createImplementationPlanRequest.Description
        };

        await unitOfWork.ImplementationPlan.AddAsync(implementationPlan);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Update(UpdateImplementationPlanDto updateImplementationPlanDto)
    {
        var implementationPlan = await unitOfWork.ImplementationPlan.GetByIdAsync(updateImplementationPlanDto.Id);

        implementationPlan.Name = updateImplementationPlanDto.Name;
        implementationPlan.Description = updateImplementationPlanDto.Description;
        await unitOfWork.ImplementationPlan.Update(implementationPlan);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Delete(int implementationPlanId)
    {
        var implementationPlan = await unitOfWork.ImplementationPlan.GetByIdAsync(implementationPlanId);

        await unitOfWork.ImplementationPlan.Delete(implementationPlan);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }
    public async Task<Result<List<ImplementationPlanDto>>> GetAll()
    {
        var implementationPlans = unitOfWork.ImplementationPlan.GetAll();
        var implementationPlanDtos = implementationPlans.Select(ip => new ImplementationPlanDto
        {
            Id = ip.Id,
            Name = ip.Name,
            Description = ip.Description
        }).ToList();

        return Result<List<ImplementationPlanDto>>.IsSuccess(implementationPlanDtos);
    }

    public async Task<Result<ImplementationPlanDto>> GetById(int implementationPlanId)
    {
        var implementationPlan = await unitOfWork.ImplementationPlan.GetByIdAsync(implementationPlanId);

        var implementationPlanDto = new ImplementationPlanDto
        {
            Id = implementationPlan.Id,
            Name = implementationPlan.Name,
            Description = implementationPlan.Description
        };

        return Result<ImplementationPlanDto>.IsSuccess(implementationPlanDto);
    }
}