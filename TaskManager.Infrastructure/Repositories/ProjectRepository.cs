using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Infrastructure.Repositories
{
    public class ProjectRepository (ApplicationDbContext context) : IProjectRepository
    {
        private readonly ApplicationDbContext _context = context;

        public void Add(Project project)
        {
            _context.Projects.Add(project);
        }

        public void Delete(Project project)
        {
            _context.Projects.Remove(project);
        }

        public async Task<IEnumerable<IProjectTile>> GetAllProjectsByOwnerIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Projects
                .Where(p => p.OwnerId == userId && p.Status != Status.Deleted)
                .Select(p => new ProjectTileDto
                {

                    Id = p.Id,
                    OwnerId = p.OwnerId,
                    Title = p.Title.Value,
                    Description = p.Description.Value,
                    TotalTodoItemCount = p.TodoItems.Count(),
                    CompleteTodoItemCount = p.TodoItems.Count(t => t.Status == Status.Complete),
                    CreatedOn = p.CreatedOn
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<Project?> GetProjectWithoutTasksAsync(Guid projectId, CancellationToken cancellationToken)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.Status != Status.Deleted, cancellationToken);
        }

        public async Task<Project?> GetProjectWithTasksAsync(Guid projectId, CancellationToken cancellationToken)
        {
            return await _context.Projects
                .Include(p => p.TodoItems)
                    .ThenInclude(t => t.Assignee)
                .FirstOrDefaultAsync(p => p.Id == projectId && p.Status != Status.Deleted, cancellationToken);
        }
        public async Task<IProjectDetailedView?> GetProjectDetailedViewAsync(Guid projectId, CancellationToken cancellationToken)
        {
            return await _context.Projects
                .Where(p => p.Id == projectId && p.Status != Status.Deleted)
                .Select(p => new ProjectDetailedViewDto
                {
                    Id = p.Id,
                    OwnerId = p.OwnerId,
                    Title = p.Title.Value,
                    Description = p.Description.Value,
                    TotalTodoItemCount = p.TodoItems.Count(),
                    CompleteTodoItemCount = p.TodoItems.Count(t => t.Status == Status.Complete),
                    CreatedOn = p.CreatedOn,
                    Status = p.Status,

                    TodoItems = p.TodoItems
                    .Select(t => new TodoItemEntry
                    {
                        Id = t.Id,
                        OwnerId = t.OwnerId, 
                        AssigneeId = t.AssigneeId,
                        Title = t.Title.Value,
                        Description = t.Description,
                        ProjectTitle = p.Title.Value,
                        AssigneeName = t.Assignee != null ? t.Assignee.FullName : string.Empty,
                        OwnerName = t.Owner != null ? t.Owner.FullName : string.Empty,
                        Priority = t.Priority ?? Priority.None,
                        DueDate = t.DueDate,
                        CreatedOn = t.CreatedOn,
                        Status = t.Status

                    }).ToList()
                }).FirstOrDefaultAsync(); ;
        }

        public void Update(Project project)
        {
            _context.Projects.Update(project);
        }
    }
}

