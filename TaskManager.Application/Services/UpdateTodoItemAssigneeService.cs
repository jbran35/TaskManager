//using Microsoft.AspNetCore.Identity;
//using TaskManager.Application.DTOs;
//using TaskManager.Application.Interfaces;
//using TaskManager.Domain.Entities;

//namespace TaskManager.Application.Services
//{
//    public class UpdateTodoItemAssigneeService : IUpdateTodoItemAssigneeService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly UserManager<User> _userManager;

//        public UpdateTodoItemAssigneeService(IUnitOfWork unitOfWork, UserManager<User> userManager)
//        {
//            _unitOfWork = unitOfWork;
//            _userManager = userManager;
//        }

//        public async Task<UpdateTodoItemAssigneeResponse> UpdateTodoItemAssigneeAsync(Guid todoItemId, Guid userId, Guid newAssigneeId)
//        {
//            //Verify input
//            if (userId == Guid.Empty || newAssigneeId == Guid.Empty || todoItemId == Guid.Empty)
//            {
//                return new UpdateTodoItemAssigneeResponse
//                {
//                    Success = false,
//                    Message = "Invalid request."
//                };
//            }

//            //Check if user exists
//            var user = await _userManager.FindByIdAsync(userId.ToString());
//            if (user == null)
//            {
//                return new UpdateTodoItemAssigneeResponse
//                {
//                    Success = false,
//                    Message = "User not found."
//                };
//            }

//            //Check if new assignee exists
//            var newAssignee = await _userManager.FindByIdAsync(newAssigneeId.ToString());
//            if (newAssignee == null)
//            {
//                return new UpdateTodoItemAssigneeResponse
//                {
//                    Success = false,
//                    Message = "New assignee not found."
//                };
//            }
            
//            //Check if task exists
//            var todoItem = await _unitOfWork.TodoItemRepository.GetTodoItemByIdAsync(todoItemId);
//            if (todoItem == null)
//            {
//                return new UpdateTodoItemAssigneeResponse
//                {
//                    Success = false,
//                    Message = "Task not found."
//                };
//            }
            
//            //Check if user is the owner of the task's project
//            var project = await _unitOfWork.ProjectRepository.GetProjectByIdAsync(todoItem.ProjectId);
//            if (project == null || project.OwnerId != userId)
//            {
//                return new UpdateTodoItemAssigneeResponse
//                {
//                    Success = false,
//                    Message = "User must be task-project owner"
//                };
//            }
            
//            //Update assignee
//            todoItem.AssignToUser(newAssigneeId, newAssignee.FirstName, newAssignee.LastName);

//            //Save changes
//            try
//            {
//                _unitOfWork.TodoItemRepository.Update(todoItem);
//                await _unitOfWork.SaveChangesAsync();
//            }
//            catch (Exception ex)
//            {
//                return new UpdateTodoItemAssigneeResponse
//                {
//                    Success = false,
//                    Message = "Could not update task assignee."
//                };
//            }
            
//            //Prepare response DTO
//            TodoItemListEntryDto taskListEntryDto = new TodoItemListEntryDto
//            {
//                ProjectId = project.Id,
//                ProjectName = project.Name.Value,
//                TodoItemId = todoItem.Id,
//                Title = todoItem.Name.Value,
//                AssigneeId = todoItem.AssigneeId,
//                AssigneeName = newAssignee.FirstName + " " + newAssignee.LastName,
//                DueDate = todoItem.DueDate,
//                Status = todoItem.Status,
//                Priority = todoItem.Priority
//            };

//            return new UpdateTodoItemAssigneeResponse
//            {
//                UpdatedTask = taskListEntryDto,
//                Success = true,
//                Message = "Task assignee updated successfully."
//            };
//        }
//    }
//}
