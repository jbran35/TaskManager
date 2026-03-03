using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.DTOs
{
    public record ProjectTileDto : IProjectTile
    {
        //Id, OwnerId, Title, Description, TotalTodoItemCount, CompleteTodoItemCount, CreatedOn, Status
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public required string Title { get; set; }

        //Included here as it would always be included upon project creation.
        //The user only needs to see the tile-displayed details, but 
        //adding it here helps with caching.
        public string? Description { get; set; } = string.Empty;
        public int TotalTodoItemCount { get; set; } = 0;
        public int CompleteTodoItemCount { get; set; } = 0;
        public DateTime CreatedOn { get; set; }
        public Status Status { get; set; } = Status.Incomplete;
    }
}