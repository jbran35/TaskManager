using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces
{
    public interface ITodoItemRepository
    {
        /// <summary>
        ///     Adds a new todo item to the repository.
        /// </summary>
        /// <param name="todoItem">  The domain Task/TodoItem entity to be created. </param>
        void Add(TodoItem todoItem);


        /// <summary>
        ///    Deletes the todo item with the specified ID from the repository.
        /// </summary>
        /// <remarks>This method will throw an exception if the todo item with the specified ID does not
        /// exist.</remarks>
        /// <param name="id">The unique identifier of the entity to be deleted. Must be a valid GUID.</param>
        void Delete(TodoItem todoItem);


        /// <summary>
        ///     Retrieves all todo items assigned to an assignee.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="assigneeId"></param>
        /// <returns>
        ///     A collection of the todo items assigned.
        /// </returns>
        Task<IReadOnlyList<ITodoItemEntry>> GetMyAssignedTodoItemsAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        ///     Retrieves a list of todo items owned by a user that are assigned to another user. 
        ///     Used to unassign todo items assigned to the assignee, when the todo item owner removes them from 
        ///     their group. 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="assigneeId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Guid>> GetMyTodoItemsAssignedToUser(Guid userId, Guid assigneeId, CancellationToken cancellationToken);


        Task UnassignTasksByIdAsync(List<Guid> todoItemIds, CancellationToken cancellationToken);

        /// <summary>
        ///     Gets the todo item by its Id.
        /// </summary>
        /// <param name="todoId"> The todo item id. </param>
        /// <returns> The task with the specified identifier. </returns>
        Task<TodoItem?> GetTodoItemByIdAsync(Guid todoId, CancellationToken cancellationToken);

        /// <summary>
        ///     Updates an existing todo item in the repository.
        /// </summary>
        /// <param name="todoItem">The task to update.</param>
        /// <returns>The updated task.</returns>
        void Update(TodoItem todoItem);


        /// <summary>
        ///     Updates a list of todo items in the repository.
        /// </summary>
        /// <param name="todoList">The task to update.</param>
        /// <returns> List of updated tasks </returns>
        void Update(IEnumerable<TodoItem> todoList);
    }
}
