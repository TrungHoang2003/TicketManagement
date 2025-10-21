using Application.DTOs;
using BuildingBlocks.Commons;
using Infrastructure.Repositories;
using Application.Erros;
using Domain.Entities;

namespace Application.Services;

public interface ICategoryService
{
    Task<Result> Create(CreateCategoryDto createCategoryDto);
    Task<Result> Update(UpdateCategoryDto updateCategoryDto);
    Task<Result> Delete(int categoryId);
    Task<Result<List<CategoryDto>>> GetAll();
    Task<Result<CategoryDto>> GetById(int categoryId);
};

public class CategoryService(IUnitOfWork unitOfWork) : ICategoryService
{
    public async Task<Result> Create(CreateCategoryDto createCategoryDto)
    {
        var category = new Category
        {
            Name = createCategoryDto.Name,
            DepartmentId = createCategoryDto.DepartmentId
        };

        await unitOfWork.Category.AddAsync(category);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Update(UpdateCategoryDto updateCategoryDto)
    {
        var category = await unitOfWork.Category.GetByIdAsync(updateCategoryDto.Id);

        category.Name = updateCategoryDto.Name;
        category.DepartmentId = updateCategoryDto.DepartmentId;
        await unitOfWork.Category.Update(category);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Delete(int categoryId)
    {
        var category = await unitOfWork.Category.GetByIdAsync(categoryId);

        await unitOfWork.Category.Delete(category);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result<List<CategoryDto>>> GetAll()
    {
        var categories = unitOfWork.Category.GetAll();
        var categoryDtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            DepartmentId = c.DepartmentId
        }).ToList();

        return Result<List<CategoryDto>>.IsSuccess(categoryDtos);
    }

    public async Task<Result<CategoryDto>> GetById(int categoryId)
    {
        var category = await unitOfWork.Category.GetByIdAsync(categoryId);

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            DepartmentId =  category.DepartmentId
        };

        return Result<CategoryDto>.IsSuccess(categoryDto);
    }

}
