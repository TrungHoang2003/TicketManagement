using Application.DTOs;
using BuildingBlocks.Commons;
using Infrastructure.Repositories;
using Application.Erros;
using Domain.Entities;


namespace Application.Services;

public interface IDepartmentService
{
    Task<Result> Create(CreateDepartmentDto createDepartmentDto);
    Task<Result> Update(UpdateDepartmentDto updateDepartmentDto);
    Task<Result> Delete(int departmentId);
    Task<Result<List<DepartmentDto>>> GetAll();
    Task<Result<DepartmentDto>> GetById(int departmentId);
};

public class DepartmentService(IUnitOfWork unitOfWork) : IDepartmentService
{
    public async Task<Result> Create(CreateDepartmentDto createDepartmentDto)
    {
        var department = new Department
        {
            Name = createDepartmentDto.Name
        };

        await unitOfWork.Department.AddAsync(department);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Update(UpdateDepartmentDto updateDepartmentDto)
    {
        var department = await unitOfWork.Department.GetByIdAsync(updateDepartmentDto.Id);

        department.Name = updateDepartmentDto.Name;
        await unitOfWork.Department.Update(department);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Delete(int departmentId)
    {
        var department = await unitOfWork.Department.GetByIdAsync(departmentId);

        await unitOfWork.Department.Delete(department);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result<List<DepartmentDto>>> GetAll()
    {
        var departments = unitOfWork.Department.GetAll();
        var departmentDtos = departments.Select(d => new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name
        }).ToList();

        return Result<List<DepartmentDto>>.IsSuccess(departmentDtos);
    }

    public async Task<Result<DepartmentDto>> GetById(int departmentId)
    {
        var department = await unitOfWork.Department.GetByIdAsync(departmentId);
        var departmentDto = new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name
        };

        return Result<DepartmentDto>.IsSuccess(departmentDto);
    }
}
