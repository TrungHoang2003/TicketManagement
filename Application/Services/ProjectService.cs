using Application.DTOs;
using BuildingBlocks.Commons;
using Infrastructure.Repositories;
using Application.Erros;
using Domain.Entities;

namespace Application.Services;

public interface IProjectService
{
    Task<Result> Create(CreateProjectRequest createProjectDto);
    Task<Result> Update(UpdateProjectRequest updateProjectDto);
    Task<Result> Delete(int projectId);
    Task<Result<List<ProjectDto>>> GetAll();
    Task<Result<ProjectDto>> GetById(int projectId);
};

public class ProjectService(IUnitOfWork unitOfWork) : IProjectService
{
    public async Task<Result> Create(CreateProjectRequest createProjectDto)
    {
        var project = new Project
        {
            Name = createProjectDto.Name,
            Description = createProjectDto.Description
        };

        await unitOfWork.Project.AddAsync(project);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Update(UpdateProjectRequest updateProjectDto)
    {
        var project = await unitOfWork.Project.GetByIdAsync(updateProjectDto.Id);

        project.Name = updateProjectDto.Name;
        project.Description = updateProjectDto.Description;
        await unitOfWork.Project.Update(project);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Delete(int projectId)
    {
        var project = await unitOfWork.Project.GetByIdAsync(projectId);
        await unitOfWork.Project.Delete(project);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result<List<ProjectDto>>> GetAll()
    {
        var projects = unitOfWork.Project.GetAll();
        var projectDtos = projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description
        }).ToList();

        return Result<List<ProjectDto>>.IsSuccess(projectDtos);
    }

    public async Task<Result<ProjectDto>> GetById(int projectId)
    {
        var project = await unitOfWork.Project.GetByIdAsync(projectId);
        var projectDto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description
        };

        return Result<ProjectDto>.IsSuccess(projectDto);
    }
}