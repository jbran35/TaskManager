using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Infrastructure
{
    public class UnitOfWork (ApplicationDbContext context) : IUnitOfWork
    {
        private readonly ApplicationDbContext _context = context;

        private IProjectRepository? _projectRepository;
        private ITodoItemRepository? _todoItemRepository;
        private IUserConnectionRepository? _userConnectionRepository;

        public IProjectRepository ProjectRepository =>
            _projectRepository ??= new ProjectRepository(_context);

        public ITodoItemRepository TodoItemRepository =>
            _todoItemRepository ??= new TodoItemRepository(_context);

        public IUserConnectionRepository UserConnectionRepository =>
            _userConnectionRepository ??= new UserConnectionRepository(_context);

        public void Dispose()
        {

            _context.Dispose();
            GC.SuppressFinalize(this);
        }
        public async Task<int> SaveChangesAsync(CancellationToken token)
        {
            return await _context.SaveChangesAsync(token);
        }
    }
}
