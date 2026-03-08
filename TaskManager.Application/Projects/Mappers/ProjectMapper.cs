using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.Mappers
{
    public static class ProjectMapper
    {

        public static ProjectDetailedViewDto? ToProjectDetailedView (this ProjectDetailsDto details)
        {
            if (details is null) return null;

            return new ProjectDetailedViewDto { 
                
                Id = details.Id,
                Title = details.Title,
                Description = details.Description,
                CreatedOn = details.CreatedOn,
            };
        }

        public static ProjectTileDto? ToProjectTileDto (this ProjectDetailsDto details) {
        
            if (details is null) return null;

            return new ProjectTileDto
            {
                Id = details.Id,
                Title = details.Title,
                Description = details.Description,
                CreatedOn = details.CreatedOn,
            };
        }

        public static ProjectTileDto? ToProjectTileDto (this ProjectDetailedViewDto project)
        {
            if (project is null) return null;

            return new ProjectTileDto
            {
                Id = project.Id, 
                OwnerId = project.OwnerId, 
                Title = project.Title,
                Description = project.Description, 
                TotalTodoItemCount = project.TotalTodoItemCount, 
                CompleteTodoItemCount = project.CompleteTodoItemCount, 
                CreatedOn = project.CreatedOn, 
                Status = project.Status
            };
        }


        public static List<ProjectTileDto>? ToProjectTileDtoList(this IEnumerable<IProjectTile> tiles)
        {
            if (tiles is null) return null;

            return tiles.Select(t => new ProjectTileDto
            {
                Id = t.Id,
                OwnerId = t.OwnerId,
                Title = t.Title,
                Description = t.Description,
                TotalTodoItemCount = t.TotalTodoItemCount,
                CompleteTodoItemCount = t.CompleteTodoItemCount,
                CreatedOn = t.CreatedOn,
                Status = t.Status
            }).ToList(); 
        }
        public static ProjectDetailedViewDto? ToProjectDetailedViewDto(this IProjectDetailedView project)
        {
            if (project is null) return null; 

            return new ProjectDetailedViewDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Status = project.Status,
                CreatedOn = project.CreatedOn,
                TotalTodoItemCount = project.TotalTodoItemCount,
                CompleteTodoItemCount = project.CompleteTodoItemCount,

                TodoItems = project.TodoItems.Select(static t => new TodoItemEntry
                {
                    Id = t.Id,
                    OwnerId = t.OwnerId,
                    AssigneeId = t.AssigneeId,
                    Title = t.Title,
                    Description = t.Description,
                    ProjectTitle = t.ProjectTitle,
                    AssigneeName = t.AssigneeName,
                    OwnerName = t.OwnerName,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedOn = t.CreatedOn,
                    Status = t.Status
                }
                ).ToList()
            };
        }
        public static ProjectDetailedViewDto? ToProjectDetailedViewDto(this Project project)
        {
            if (project is null) return null;

            var detailedDto = new ProjectDetailedViewDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Status = project.Status,
                CreatedOn = project.CreatedOn
            }; 


            if (project.TodoItems is null || project.TodoItems.Count == 0)
            {
                return detailedDto; 
            }

            detailedDto.TodoItems = project.TodoItems.Select(static t => new TodoItemEntry
            {
                Id = t.Id,
                OwnerId = t.OwnerId,
                AssigneeId = t.AssigneeId,
                Title = t.Title,
                Description = t.Description,
                ProjectTitle = t.Project.Title,
                AssigneeName = t.Assignee?.FullName ?? string.Empty,
                OwnerName = t.Owner?.FullName ?? string.Empty,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CreatedOn = t.CreatedOn,
                Status = t.Status
            }).ToList();

            return detailedDto; 
        }

        public static ProjectDetailsDto? ToProjectDetailsDto(this Project project)
        {
            if (project is null) return null;

            return new ProjectDetailsDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
            };
        }
    }
}
