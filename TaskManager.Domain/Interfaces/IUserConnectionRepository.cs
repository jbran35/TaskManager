using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces
{
    public interface IUserConnectionRepository
    {

        /// <summary>
        ///     Adds a new user connection.
        /// </summary>
        /// <param name="userId, assigneeId">  The Id for the user and the new assignee </param>
        void Add(UserConnection connection);


        /// <summary>
        ///     Used to verify whether two users are connected already.
        /// </summary>
        /// 
        /// <param name="userId"> Id for the requesting user </param>
        /// <param name="assigneeId"> Id for the other user </param>
        /// <returns> a bool representing whether they are connected (true) already or not (false) </returns>
        Task<bool> AnyConnectionExistsAsync(Guid userId, Guid assigneeId, CancellationToken cancellationToken);


        /// <summary>
        ///     Delete's a user connection.
        /// </summary>
        /// 
        /// <param name="userId"> The ID for the user </param>
        /// <param name="assigneeId"> The ID for the assignee they are deleting their connection with </param>
        void Delete(UserConnection connection);


        /// <summary>
        /// Asynchronously retrieves the user connection associated with the specified unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user connection to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user connection if found;
        /// otherwise, <see langword="null"/>.</returns>
        Task<UserConnection?> GetConnectionByIdAsync(Guid id, CancellationToken cancellationToken);


        /// <summary>
        ///     Retrieves a user's connections/assignees.
        /// </summary>
        /// 
        /// <param name="userId">The identifier of the requesting user. </param>
        /// 
        /// <returns> A list of the user connections </returns>
        Task<IEnumerable<UserConnection>> GetConnectionsByOwnerIdAsync(Guid userId, CancellationToken cancellationToken);


        /// <summary>
        /// Asynchronously retrieves the user connection associated with the specified user and assignee identifiers.
        /// </summary>
        /// 
        /// <remarks>This method is intended for use in asynchronous contexts. Ensure that both
        /// identifiers are valid GUIDs to avoid exceptions.</remarks>
        /// 
        /// <param name="userId">The unique identifier of the user whose connection is to be retrieved. Cannot be null.</param>
        /// <param name="assigneeId">The unique identifier of the assignee related to the user connection. Cannot be null.</param>
        /// 
        /// <returns>A task that represents the asynchronous operation. The task result contains the user connection if found;
        /// otherwise, null.</returns>
        Task<UserConnection?> GetConnectionByUserIdsAsync(Guid userId, Guid assigneeId, CancellationToken cancellationToken);
    }
}
