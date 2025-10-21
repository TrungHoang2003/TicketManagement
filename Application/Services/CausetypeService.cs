using Application.DTOs;
using BuildingBlocks.Commons;
using Infrastructure.Repositories;
using Application.Erros;
using Domain.Entities;

namespace Application.Services;

public interface ICauseTypeService
{
    Task<Result> Create(CreateCauseTypeDto createCauseTypeDto);
    Task<Result> Update(UpdateCauseTypeDto updateCauseTypeDto);
    Task<Result> Delete(int causeTypeId);
    Task<Result<List<CauseTypeDto>>> GetAll();
    Task<Result<CauseTypeDto>> GetById(int causeTypeId);
};

public class CauseTypeService(IUnitOfWork unitOfWork) : ICauseTypeService
{
    public async Task<Result> Create(CreateCauseTypeDto createCauseTypeDto)
    {
        var causeType = new CauseType
        {
            Name = createCauseTypeDto.Name,
            Description = createCauseTypeDto.Description
        };

        await unitOfWork.CauseType.AddAsync(causeType);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Update(UpdateCauseTypeDto updateCauseTypeDto)
    {
        var causeType = await unitOfWork.CauseType.GetByIdAsync(updateCauseTypeDto.Id);

        causeType.Name = updateCauseTypeDto.Name;
        causeType.Description = updateCauseTypeDto.Description;
        await unitOfWork.CauseType.Update(causeType);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Delete(int causeTypeId)
    {
        var causeType = await unitOfWork.CauseType.GetByIdAsync(causeTypeId);

        await unitOfWork.CauseType.Delete(causeType);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result<List<CauseTypeDto>>> GetAll()
    {
        var causeTypes = unitOfWork.CauseType.GetAll();
        var causeTypeDtos = causeTypes.Select(ct => new CauseTypeDto
        {
            Id = ct.Id,
            Name = ct.Name,
            Description = ct.Description
        }).ToList();

        return Result<List<CauseTypeDto>>.IsSuccess(causeTypeDtos);
    }

    public async Task<Result<CauseTypeDto>> GetById(int causeTypeId)
    {
        var causeType = await unitOfWork.CauseType.GetByIdAsync(causeTypeId);

        var causeTypeDto = new CauseTypeDto
        {
            Id = causeType.Id,
            Name = causeType.Name,
            Description = causeType.Description
        };

        return Result<CauseTypeDto>.IsSuccess(causeTypeDto);
    }
}