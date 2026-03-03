namespace TaskManager.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProjectRepository ProjectRepository { get; }
        ITodoItemRepository TodoItemRepository { get; }
        IUserConnectionRepository UserConnectionRepository { get; }



        /// <summary>
        ///     Saves all changes made in the unit of work to the underlying data store.
        /// </summary>
        /// <returns> The number of state entries written to the underlying database. </returns>
        Task<int> SaveChangesAsync(CancellationToken token);

        /// <summary>
        ///     Disposes the unit of work, releasing any resources it holds.
        /// </summary>
        //void Dispose();
    }
}
