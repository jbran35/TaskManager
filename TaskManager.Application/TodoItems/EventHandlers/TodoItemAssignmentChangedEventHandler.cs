using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Application.TodoItems.Events;

namespace TaskManager.Application.TodoItems.EventHandlers
{
    public class TodoItemAssignmentChangedEventHandler(IDistributedCache cache, ITodoItemUpdateNotificationService updateNotificationService)
        : INotificationHandler<TodoItemAssignmentChangedEvent>
    {
        private readonly IDistributedCache _cache = cache;
        private readonly ITodoItemUpdateNotificationService _updateNotificationService = updateNotificationService; 
        public async Task Handle(TodoItemAssignmentChangedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.OldAssigneeId is not null && notification.OldAssigneeId != Guid.Empty)
            {
                await _cache.RemoveAsync(CacheKeys.AssignedTodoItems(notification.OldAssigneeId.Value), CancellationToken.None);
                await _updateNotificationService.NotifyTodoItemUpdated(notification.OldAssigneeId.Value.ToString());
            }

            if(notification.NewAssigneeId is not null && notification.NewAssigneeId != Guid.Empty && notification.NewAssigneeId != notification.OldAssigneeId)
            {
                await _cache.RemoveAsync(CacheKeys.AssignedTodoItems(notification.NewAssigneeId.Value), CancellationToken.None);
                await _updateNotificationService.NotifyTodoItemUpdated(notification.NewAssigneeId.Value.ToString()); 
            }
        }
    }
}
