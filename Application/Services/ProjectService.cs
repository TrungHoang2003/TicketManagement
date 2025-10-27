using Application.DTOs;
using BuildingBlocks.Commons;
using Infrastructure.Repositories;
using Application.Erros;
using Domain.Entities;

namespace Application.Services;

public interface IProjectService
{
    Task<Result> Create(CreateProjectRequest createProjectRequest);
    Task<Result> Update(UpdateProjectRequest updateProjectRequest);
    Task<Result> Delete(int projectId);
    Task<Result<List<ProjectDto>>> GetAll();
    Task<Result<ProjectDto>> GetById(int projectId);
};

public class ProjectService(IUnitOfWork unitOfWork) : IProjectService
{
    public async Task<Result> Create(CreateProjectRequest createProjectRequest)
    {
        var project = new Project
        {
            Name = createProjectRequest.Name,
            Description = createProjectRequest.Description
        };

        await unitOfWork.Project.AddAsync(project);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Update(UpdateProjectRequest updateProjectRequest)
    {
        var project = await unitOfWork.Project.GetByIdAsync(updateProjectRequest.Id);
        if (project == null)
        {
            return new Error("Not Found", "Project not found");
        }

        project.Name = updateProjectRequest.Name;
        project.Description = updateProjectRequest.Description;
        await unitOfWork.Project.Update(project);
        await unitOfWork.SaveChangesAsync();
        return Result.IsSuccess();
    }

    public async Task<Result> Delete(int projectId)
    {
        var project = await unitOfWork.Project.GetByIdAsync(projectId);
        if (project == null)
        {
            return new Error("Not Found", "Project not found");
        }

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
        if (project == null)
        {
            return new Error("Not Found", "Project not found");
        }

        var projectDto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description
        };

        return Result<ProjectDto>.IsSuccess(projectDto);
    }
}