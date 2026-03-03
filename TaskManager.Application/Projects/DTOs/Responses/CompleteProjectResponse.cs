using TaskManager.Domain.Enums;

namespace TaskManager.Application.Projects.DTOs.Responses
{
    //A record to represent the response after requesting to completing a project
    public record CompleteProjectResponse(ProjectTileDto ProjectTile);
        
}
