using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using TaskManager.Application.UserConnections.DTOs;

namespace TaskManager.Presentation.Services
{
    public class AssigneeListStateService(IMemoryCache cache, AuthenticationStateProvider authStateProvider)
    {
        private readonly IMemoryCache _cache = cache;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

        public event Action? OnChange;

        private async Task<string> GetMyAssigneesKey() => $"assignees:{await GetUserIdAsync()}";
        private void NotifyStateChanged() => OnChange?.Invoke();
        private async Task<string> GetUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        }

        //--------------------Getters--------------------
        public async Task<List<UserConnectionDto>?> GetAssigneesFromCacheAsync()
        {
            var original = _cache.Get<List<UserConnectionDto>>(await GetMyAssigneesKey());
            return original is not null ? Clone(original) : null;
        }

        public async Task<UserConnectionDto?> GetAssigneeFromCacheAsync(Guid connectionId) 
        {
            var originalList = _cache.Get<List<UserConnectionDto>>(await GetMyAssigneesKey());
            if (originalList is null) return null; 

            var original = originalList.FirstOrDefault(uc => uc.Id == connectionId);

            return original is not null ? Clone(original) : null; 
        }

        //--------------------Removers--------------------
        public async Task RemoveFromCacheAsync(UserConnectionDto connection)
        {
            if (connection is null) return; 

            var assignees = await GetAssigneesFromCacheAsync();
            if (assignees is null) return; 

            var index = assignees.FindIndex(uc =>  uc.Id == connection.Id);
            if (index == -1) return;

            assignees.RemoveAt(index);
            await SetAssigneesInCacheAsync(assignees, false);
            NotifyStateChanged(); 
        }

        //--------------------Setters--------------------
        public async Task SetAssigneesInCacheAsync(List<UserConnectionDto> assignees, bool notify = true)
        {
            if (assignees is null) return;

            var options = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(20))
              .SetSize(1);

            var key = await GetMyAssigneesKey();

            _cache.Set(key, assignees, options);

            if (notify) NotifyStateChanged();

        }
        public async Task SetAssigneeInCacheAsync(UserConnectionDto connection)
        {
            if (connection is null) return;

            var connections = await GetAssigneesFromCacheAsync();
            if (connections is null) return; 

            var index = connections.FindIndex(c => c.Id == connection.Id);
            if (index != -1) // Assignee already exists - overwrite for the sake of potential future implementation where the user can edit their assignees
                connections[index] = connection;
            
            else
                connections.Add(connection);

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20))
                .SetSize(1);

            _cache.Set(await GetMyAssigneesKey(), connections, options);
            NotifyStateChanged();
        }

        //--------------------Cloners--------------------
        public List<UserConnectionDto>? Clone(List<UserConnectionDto> original)
        {
            if (original is null) return null;

            return original.Select(uc => new UserConnectionDto
            {
                Id = uc.Id,
                UserId = uc.UserId, 
                AssigneeId = uc.AssigneeId,
                AssigneeName = uc.AssigneeName,
                AssigneeEmail = uc.AssigneeEmail,
            }).ToList();
        }

        public UserConnectionDto? Clone(UserConnectionDto original)
        {
            if (original is null) return null;

            return new UserConnectionDto
            {
                Id = original.Id,
                UserId = original.UserId,
                AssigneeId = original.AssigneeId,
                AssigneeName = original.AssigneeName,
                AssigneeEmail = original.AssigneeEmail,
            };
        }

        //--------------------Clear--------------------
        public async Task ClearAsync()
        {
            _cache.Remove(await GetMyAssigneesKey());
            NotifyStateChanged();
        }
    }
}
