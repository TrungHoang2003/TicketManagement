using Application.DTOs;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Commons;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface ICommentService: IGenericRepository<Comment>
{
    Task<Result<List<CommentDto>>> GetTicketComments(int ticketId);
    Task<Result> CreateComment(CreateCommentRequest request);
}

public class CommentService(AppDbContext dbContext, IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService) : GenericRepository<Comment>(dbContext), ICommentService
{
    public async Task<Result<List<CommentDto>>> GetTicketComments(int ticketId)
    {
        var comments = await unitOfWork.Comment.GetAll()
            .Where(c => c.TicketId == ticketId)
            .Select(c => new CommentDto
            {
                TicketId = c.TicketId,
                Content = c.Content,
                CreatedDate = c.CreatedDate,
                AttachmentUrls = unitOfWork.Attachment.GetAll()
                    .Where(a => a.EntityType == EntityType.Comment && a.EntityId == c.Id)
                    .Select(a => a.Url)
                    .ToList()
            })
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
        
        return comments;
    }

    public async Task<Result> CreateComment(CreateCommentRequest request)
    {
        var comment = new Comment
        {
            TicketId = request.TicketId,
            Content = request.Content,
            CreatedDate = DateTime.UtcNow
        };

        await unitOfWork.Comment.AddAsync(comment);
        await unitOfWork.SaveChangesAsync();

        if (request.Base64Files != null && request.Base64Files.Count != 0)
        {
            var result = await cloudinaryService.UploadFiles(request.Base64Files);
            var attachments = new List<Attachment>();

            foreach (var url in result)
            { 
                var attachment = new Attachment
                {
                    EntityId = comment.Id,
                    EntityType = EntityType.Comment,
                    ContentType = "image/png",
                    Url = url
                };
                attachments.Add(attachment);
            }
            await unitOfWork.Attachment.AddRangeAsync(attachments);
            await unitOfWork.SaveChangesAsync();
        }
        return Result.IsSuccess();
    }
}