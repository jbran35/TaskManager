using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Infrastructure.Repositories
{
    public class UserConnectionRepository(ApplicationDbContext context) : IUserConnectionRepository
    {
        private readonly ApplicationDbContext _context = context;

        public void Add(UserConnection connection)
        {
            _context.Add(connection);
        }

        public async Task<bool> AnyConnectionExistsAsync(Guid userId, Guid assigneeId, CancellationToken cancellationToken)
        {
            return await _context.UserConnections
            .AnyAsync(uc => uc.UserId == userId && uc.AssigneeId == assigneeId, cancellationToken); 
        }

        public async Task<UserConnection?> GetConnectionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.UserConnections.FirstOrDefaultAsync(uc => uc.Id == id, cancellationToken);
        }

        public async Task<UserConnection?> GetConnectionByUserIdsAsync(Guid userId, Guid assigneeId, CancellationToken cancellationToken)
        {
            return await _context.UserConnections
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.AssigneeId == assigneeId, cancellationToken);
        }

        public async Task<IEnumerable<UserConnection>> GetConnectionsByOwnerIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserConnections
                .Include(uc => uc.Assignee)
                .Where(uc => uc.UserId == userId)
                .ToListAsync(cancellationToken); 
        }

        public void Delete(UserConnection connection)
        {
            _context.Remove(connection); 
        }
    }
}
