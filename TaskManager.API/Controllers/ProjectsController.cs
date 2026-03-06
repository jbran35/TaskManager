using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Application.DTOs.Requests;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.DTOs.Requests;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Application.Projects.Queries;
using TaskManager.Application.TodoItems.DTOs;


namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController (IMediator mediator, ILogger<ProjectsController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<ProjectsController> _logger = logger;

        //Endpoint to mark project as complete.
        [HttpPatch("complete/{projectId}")]
        public async Task<ActionResult<CompleteProjectResponse>> Complete([FromRoute] Guid projectId)
        {
            //Validate User Identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { Message = "User ID not found in token" });

            var userId = Guid.Parse(userIdString);
            var command = new CompleteProjectCommand(userId, projectId);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }

        //Endpoint to create new project
        [HttpPost]
        public async Task<ActionResult<ProjectTileDto>> Create([FromBody] CreateProjectRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || request is null)
                return Unauthorized(new { Message = "Unauthorized" });

            var userId = Guid.Parse(userIdString);
            var command = new CreateProjectCommand(userId, request.Title, request.Description);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }

        //Endpoint to delete project
        [HttpDelete("{projectId}")]
        public async Task<ActionResult<DeleteProjectResponse>> Delete([FromRoute] Guid projectId)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { Message = "User ID not found in token" });

            var userId = Guid.Parse(userIdString);
            var command = new DeleteProjectCommand(userId, projectId);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }

        //Endpoint to get detailed view of project after clicking project's tile
        [HttpGet("{projectId}")]
        public async Task<ActionResult<GetProjectDetailedViewResponse>> GetProjectDetailedView([FromRoute] Guid projectId)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { Message = "User ID not found in token" });

            var userId = Guid.Parse(userIdString);
            var command = new GetProjectDetailedViewQuery(userId, projectId);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }

        //Endpoint to get details of project when editing.
        [HttpGet("{projectId}/details")]
        public async Task<ActionResult<ProjectDetailsDto>> GetProjectDetailsAsync([FromRoute] Guid projectId)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { Message = "User ID not found in token" });

            var userId = Guid.Parse(userIdString);
            var command = new GetProjectDetailsQuery(userId, projectId);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }


        //[HttpGet("Tile/{projectId}")]
        //public async Task<ActionResult<GetProjectTileViewResponse>> GetProjectTile([FromRoute] Guid projectId)
        //{
        //    //Validate user identity
        //    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (string.IsNullOrEmpty(userIdString))
        //    {
        //        return Unauthorized(new { Message = "User ID not found in token" });
        //    }

        //    var userId = Guid.Parse(userIdString);

        //    //Call service to get project detailed view
        //    var response = await _getProjectTileViewService.GetProjectTileViewAsync(projectId, userId);

        //    if (response is null)
        //    {
        //        return BadRequest(response);
        //    }
        //    return Ok(response);
        //}


        //Endpoint to get all projects associated with user in tile format for display on user's homepage
        [HttpGet("MyProjects")]
        public async Task<ActionResult<List<Application.Projects.DTOs.ProjectTileDto>>> GetUserProjects()
        {
            _logger.LogInformation("In MyProjects Endpoint"); 

            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { Message = "User ID not found in token" });

            var userId = Guid.Parse(userIdString);
            var usersName = User.FindFirstValue(ClaimTypes.Name);
            var command = new GetUserProjectsQuery(userId);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }

        //Endpoint to retrieve the Project Detailed View (retrieving tasks & project details)
        [HttpGet("{projectId}/tasks")]
        public async Task<ActionResult<ProjectDetailedViewDto>> GetProjectTodoItems([FromRoute] Guid projectId)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { Message = "User ID not found in token" });

            var userId = Guid.Parse(userIdString);
            var query = new GetProjectDetailedViewQuery(userId, projectId);
            var result = await _mediator.Send(query);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }


        //Endpoint to update project details such as name and description
        [HttpPatch("{projectId}")]
        public async Task<ActionResult<ProjectDetailsDto>> Update([FromRoute] Guid projectId, [FromBody] UpdateProjectRequest request)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { Message = "User ID not found in token" });

            var userId = Guid.Parse(userIdString);
            var command = new UpdateProjectCommand(userId, projectId, request.Title, request.Description);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }


        //Adds a todo item to a project
        [HttpPost("{projectId}/tasks")]

        public async Task<ActionResult<TodoItemEntry>> AddTodoItem([FromRoute] Guid projectId, [FromBody] CreateTodoItemRequest request)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { Message = "User ID not found in token" });

            var userId = Guid.Parse(userIdString);
            var command = new AddTodoItemCommand(projectId, userId, request.AssigneeId, request.Title, request.Description, request.DueDate, request.Priority);
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }
    }
}
