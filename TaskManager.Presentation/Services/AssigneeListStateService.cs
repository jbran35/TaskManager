using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.UserConnections.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Presentation.Services
{
    public class AssigneeListStateService(IMemoryCache cache, AuthenticationStateProvider authStateProvider)
    {
        private readonly IMemoryCache _cache = cache;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider; 
        private List<UserConnectionDto> _assigneeCache = new List<UserConnectionDto>();

        private async Task<string> GetUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            Console.WriteLine("IN PROJECTSTATESERVICE, USERID FOUND: " + authState.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");
            return authState.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        }
        private async Task<string> GetMyGroupKey(Guid userId) => $"assignees:{userId}";

        public List<UserConnectionDto>? GetAssigneesFromCache()
        {
            //var original = _cache.Get<List<UserConnectionDto>>(await GetDetailsKey(projectId));

            if (_assigneeCache.Any())
            {
                return _assigneeCache;
            }
            return null;
        }
         
        public void SetAssigneesInCache(List<UserConnectionDto> assignees)
        {
            if(assignees.Any())
            {
                _assigneeCache = assignees;
            }
        }

        public void SetAssigneeInCache(UserConnectionDto assignee)
        {
            if (assignee is not null)
            {
                _assigneeCache.Add(assignee);
            }
        }

        public void RemoveFromCache(UserConnectionDto connection)
        {
             _assigneeCache.Remove(connection);
        }
    }
}
