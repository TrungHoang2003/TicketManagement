﻿using Infrastructure.Database;

namespace Infrastructure.Repositories;

public interface IUnitOfWork
{
   ITicketRepository Ticket { get; }
   IUserRepository User { get; }
   IDepartmentRepository Department { get; }
   ICategoryRepository Category { get; }
   IAttachmentRepository Attachment { get; }
   ICommentRepository Comment { get; }
   IHistoryRepository History { get; }
   IProjectRepository Project { get; }
   ICauseTypeRepository CauseType { get; }
   IImplementationPlanRepository ImplementationPlan { get; }
   IProgressRepository Progress { get; }

   Task SaveChangesAsync();
}

public class UnitOfWork(AppDbContext dbContext, ITicketRepository ticket, IUserRepository user, IDepartmentRepository department, ICategoryRepository category, IAttachmentRepository attachment, ICommentRepository comment, IHistoryRepository history, IProgressRepository progress, IProjectRepository project, ICauseTypeRepository causeType, IImplementationPlanRepository implementationPlan) : IUnitOfWork
{
   public ITicketRepository Ticket { get; } = ticket;
   public IUserRepository User { get; } = user;
   public IDepartmentRepository Department { get; } = department;
   public ICategoryRepository Category { get; } = category;
   public IAttachmentRepository Attachment { get; } = attachment;
   public ICommentRepository Comment { get; } = comment;
   public IHistoryRepository History { get; } = history;
   public IProgressRepository Progress { get; } = progress;
   public IProjectRepository Project { get; } = project;
   public ICauseTypeRepository CauseType { get; } = causeType;
   public IImplementationPlanRepository ImplementationPlan { get; } = implementationPlan;


   public async Task SaveChangesAsync()
   {
      await dbContext.SaveChangesAsync();
   }
}