using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TaskManager.API.Hubs;
using TaskManager.Application.TodoItems.Commands;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Application.TodoItems.DTOs.Requests;
using TaskManager.Application.TodoItems.DTOs.Responses;
using TaskManager.Application.TodoItems.Queries;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class TodoItemsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator; 

        //Endpoint to mark a task as complete. 
        [HttpPatch("UpdateStatus/{todoItemId}")]
        public async Task<ActionResult<TodoItemEntry>> UpdateTodoItemStatus([FromRoute] Guid todoItemId)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var userId = Guid.Parse(userIdString);

            var command = new UpdateTodoItemStatusCommand(userId, todoItemId);

            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }

        //Endpoint to create a new task.
        //[HttpPost]
        //public async Task<ActionResult<CreateTodoItemResponse>> CreateTodoItemAsync([FromBody] CreateTodoItemRequest request)
        //{

        //    //Validate user identity
        //    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (string.IsNullOrEmpty(userIdString))
        //    { 
        //        return Unauthorized(new { Message = "User ID not found in token" });
        //    }

        //    var userId = Guid.Parse(userIdString);

        //    var command = new Cre(userId, todoItemId);

        //    var result = await _mediator.Send(command);

        //    return result.IsSuccess ? Ok(result.SuccessMessage) : BadRequest(result.ErrorMessage);
        //}


        //Endpoint to delete a task.
        [HttpDelete("{todoItemId}")]
        public async Task<ActionResult> DeleteTodoItemAsync([FromRoute] Guid todoItemId)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var userId = Guid.Parse(userIdString);

            var command = new DeleteTodoItemCommand(userId, todoItemId);

            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.SuccessMessage) : BadRequest(result.ErrorMessage);
        }

        //Endpoint to get all tasks assigned to the user.
        [HttpGet("MyAssignedTasks")]
        public async Task<ActionResult<List<TodoItemEntry>>> GetAssignedTodoItems()
        {

            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var userId = Guid.Parse(userIdString);

            var query = new GetAssignedTodoItemsQuery(userId);

            var result = await _mediator.Send(query);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }


        //Endpoint to get detailed view of a task when clicked on the ProjectDetailedViews page
        [HttpGet("{todoItemId}")]
        public async Task<ActionResult<TodoItemEntry>> GetTodoItemDetailedView([FromRoute] Guid todoItemId)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var userId = Guid.Parse(userIdString);

            var query = new GetTodoItemDetailedViewQuery(todoItemId, userId);

            var result = await _mediator.Send(query);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }


        //Endpoint to update task details.
        [HttpPatch("{todoItemId}")]

        public async Task<ActionResult<TodoItemEntry>> UpdateTodoItem([FromRoute] Guid todoItemId, [FromBody] UpdateTodoItemRequest request, IHubContext<TaskHub> hubContext)
        {
            //Validate user identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var userId = Guid.Parse(userIdString);

            var command = new UpdateTodoItemCommand(userId, request.ProjectId, todoItemId, request.AssigneeId, request.Title, request.Description, 
                request.Priority, request.DueDate);

            var result = await _mediator.Send(command);

            //await hubContext.Clients.All.SendAsync("TodoItem Updated Successful (hub context)"); 

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);

            }

            //Endpoint to update the project assignment of a task. 

        //[HttpPost("UpdateTaskProject/{todoItemId}")]
        //public async Task<ActionResult<UpdateTodoItemProjectAssignmentResponse>> UpdateTodoItemProjectAssignment([FromRoute] Guid todoItemId, [FromBody] UpdateTodoItemProjectAssignmentRequest request)
        //{
        //    //Validate user identity
        //    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (string.IsNullOrEmpty(userIdString))
        //    {
        //        return Unauthorized(new { Message = "User ID not found in token" });
        //    }

        //    var userId = Guid.Parse(userIdString);

        //    //Call service to update task project assignment
        //    var response = await _updateTodoItemProjectAssignementService.UpdateTodoItemProjectAssignmentAsync(request, userId, todoItemId);

        //    if (response is null)
        //    {
        //        return BadRequest(response);
        //    }

        //    return Ok(response);
        //}
    }
}
