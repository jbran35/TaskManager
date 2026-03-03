using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces
{
    public interface IProjectRepository
    {
        /// <summary>
        ///     Adds a new project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns> </returns>
        void Add(Project project);


        /// <summary>
        /// Deletes the project identified by the specified project ID.
        /// </summary>
        /// <remarks>This method will throw an exception if the project ID does not correspond to an
        /// existing project.</remarks>
        /// <param name="projectId">The unique identifier of the project to be deleted. This value must be a valid GUID representing an existing
        /// project.</param>
        /// <returns>A task that represents the asynchronous operation of deleting the project.</returns>
        void Delete(Project projectId);



        /// <summary>
        ///     Retrieves all user projects, based on the user's ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<IProjectTile>> GetAllProjectsByOwnerIdAsync(Guid userId, CancellationToken cancellationToken);


        /// <summary>
        /// Retrieves the project with the specified identifier, excluding any associated tasks.
        /// </summary>
        /// <remarks>Use this method when only the project details are required, as it avoids the overhead
        /// of loading related tasks.</remarks>
        /// <param name="projectId">The unique identifier of the project to retrieve. Must be a valid <see cref="System.Guid"/> representing an
        /// existing project.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Project"/> if
        /// found; otherwise, <see langword="null"/>.</returns>
        Task<Project?> GetProjectWithoutTasksAsync(Guid projectId, CancellationToken cancellationToken);


        /// <summary>
        ///     Retrieves the project that matches the specified identifier, including all associated tasks.
        /// </summary>
        /// 
        /// <remarks>Returns null if no project exists with the specified identifier. Ensure that the
        /// projectId is valid before calling this method.</remarks>
        /// 
        /// <param name="projectId">The unique identifier of the project to retrieve. Must be a valid GUID corresponding 
        /// to an existing project.</param>
        /// 
        /// <returns>A task that represents the asynchronous operation. The task result contains the project with its related
        /// tasks if found; otherwise, null.</returns>
        Task<Project?> GetProjectWithTasksAsync(Guid projectId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves detailed information for the specified project asynchronously.
        /// </summary>
        /// <remarks>Throws an exception if projectId is invalid or if the operation is
        /// canceled.</remarks>
        /// <param name="projectId">The unique identifier of the project for which detailed information is requested. Must be a valid GUID.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an instance of
        /// IProjectDetailedView with the project's detailed information, or null if the project is not found.</returns>
        Task<IProjectDetailedView?> GetProjectDetailedViewAsync(Guid projectId, CancellationToken cancellationToken);

        /// <summary>
        ///     Updates a project's details (e.g., name, description, etc.).
        /// </summary>
        /// <param name="project"></param>
        void Update(Project project);
    }
}
