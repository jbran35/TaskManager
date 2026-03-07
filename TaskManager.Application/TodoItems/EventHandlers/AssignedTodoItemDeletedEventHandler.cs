using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.TodoItems.Events;

namespace TaskManager.Application.TodoItems.EventHandlers
{
    public class AssignedTodoItemDeletedEventHandler(IDistributedCache cache, ITodoItemUpdateNotificationService updateNotificationService)
        : INotificationHandler<AssignedTodoItemDeletedEvent>
    {
        private readonly IDistributedCache _cache = cache;
        private readonly ITodoItemUpdateNotificationService _updateNotificationService = updateNotificationService;
        public async Task Handle(AssignedTodoItemDeletedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.AssigneeId is not null && notification.AssigneeId != Guid.Empty)
            {
                await _cache.RemoveAsync(CacheKeys.AssignedTodoItems(notification.AssigneeId.Value), CancellationToken.None);
                await _updateNotificationService.NotifyTodoItemUpdated(notification.AssigneeId.Value.ToString());
            }
        }
    }
}
