using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class DeleteAssigneeService : IDeleteAssigneeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public DeleteAssigneeService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<DeleteAssigneeResponse> DeleteAssigneeAsync(Guid userId, Guid assigneeId)
        {
            //Validate Input
            if (assigneeId == Guid.Empty || userId == Guid.Empty)
            {
                return (new DeleteAssigneeResponse
                {
                    Success = false,
                    Message = "Missing ID for assignee or user."
                });
            }

            //Retrieve Users
            var user = _userManager.FindByIdAsync(userId.ToString());
            var assignee = _userManager.FindByIdAsync(assigneeId.ToString());
            Console.WriteLine("Retrieved users in delete assignee");

            //Validate Users
            if (user is null)
            {
                return (new DeleteAssigneeResponse
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            if (assignee is null)
            {
                return (new DeleteAssigneeResponse
                {
                    Success = false,
                    Message = "Assignee not found."
                });
            }

            //Search For Existing Connection
            var isConnected = await _unitOfWork.UserConnectionRepository.FindConnection(userId, assigneeId);

            //If the connection exists, delete it
            if (isConnected)
            {

                //Load assignees tasks
                var assigneeTasks = await _unitOfWork.TodoItemRepository.GetAssigneeTasksAsync(userId, assigneeId);

                try
                {
                    if (assigneeTasks != null && assigneeTasks.Count() >= 1)
                    {
                        foreach (var task in assigneeTasks)
                        {
                            task.AssigneeId = null;

                        }
                        _unitOfWork.TodoItemRepository.Update(assigneeTasks);

                    }

                    await _unitOfWork.UserConnectionRepository.Delete(userId, assigneeId);
                    await _unitOfWork.SaveChangesAsync();

                    Console.WriteLine("Returning successful delete assignee response");
                    return (new DeleteAssigneeResponse
                    {
                        DeletedAssigneeId = assigneeId,
                        Success = true,
                        Message = "Assignee deleted successfully."
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return (new DeleteAssigneeResponse
            {
                Success = false,
                Message = "Assignee not connected with your account."
            });
        }
    }
}
