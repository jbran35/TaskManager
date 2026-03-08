using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.Mappers;
using TaskManager.Application.TodoItems.DTOs;
namespace TaskManager.Presentation.Services
{
    public class ProjectStateService(IMemoryCache cache, AuthenticationStateProvider authStateProvider)
    {

        private readonly IMemoryCache _cache = cache;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

        public event Action? OnChange;
        private async Task<string> GetUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        }

        private async Task<string> GetDetailsKey(Guid projectId) => $"{await GetUserIdAsync()}_project_details_{projectId}";
        private async Task<string> GetTilesKey() => $"project_tiles_{await GetUserIdAsync()}";
        private void NotifyStateChanged() => OnChange?.Invoke();

        //----------- Getters ----------- 

        public async Task<ProjectDetailedViewDto?> GetProjectDetails(Guid projectId)
        {
            var original = _cache.Get<ProjectDetailedViewDto>(await GetDetailsKey(projectId));
            return original is not null ? Clone(original) : null;
        }

        public async Task<List<ProjectTileDto>?> GetUserProjectTiles()
        {
            var original = _cache.Get<List<ProjectTileDto>>(await GetTilesKey());
            return original is not null ? Clone(original) : null;

        }

        public async Task<TodoItemEntry?> GetTodoItem(Guid projectId, Guid todoItemId)
        {
            var original = _cache.Get<ProjectDetailedViewDto>(await GetDetailsKey(projectId))?
                .TodoItems
                .FirstOrDefault(t => t.Id == todoItemId);

            return original is not null ? Clone(original) : null;
        }

        public async Task<ProjectTileDto?> GetProjectTile(Guid projectId)
        {
            var original = _cache
                .Get<List<ProjectTileDto>>(await GetTilesKey())?
                .FirstOrDefault(p => p.Id == projectId);

            return original is not null ? Clone(original) : null;
        }

        public async Task<ProjectDetailsDto?> GetProjectBasicDetails(Guid projectId)
        {
            var cachedProject = _cache.Get<ProjectDetailedViewDto>(await GetDetailsKey(projectId));

            return cachedProject == null ? null : new ProjectDetailsDto
            {
                Id = cachedProject.Id,
                Title = cachedProject.Title,
                Description = cachedProject.Description,
                CreatedOn = cachedProject.CreatedOn
            };
        }

        public async Task<List<TodoItemEntry>?> GetProjectTodoItems(Guid projectId)
        {
            var original = _cache.Get<ProjectDetailedViewDto>(await GetDetailsKey(projectId));

            if (original is null) return null; 

            return Clone(original)?.TodoItems ?? null; 
        }


        //----------- Setters ----------- 
        public async Task SetProjectBasicDetails(ProjectDetailsDto details)
        {
            if (details is null) return;

            //Details
            var project = await GetProjectDetails(details.Id);
            if (project is null)
            {
                var projDetails = details.ToProjectDetailedView();

                if (projDetails is not null)
                {
                    await SetAllProjectDetails(projDetails, false);
                }
            }

            else
            {
                project.Title = details.Title;
                project.Description = details.Description;
                await SetAllProjectDetails(project, false);
            }
        
            var tile = await GetProjectTile(details.Id);
            if (tile is null)
            {
                var projectTile = details.ToProjectTileDto();
                if (projectTile is not null) 
                    await SetProjectTile(projectTile, false);
            }
            else 
            {
                tile.Title = details.Title;
                tile.Description = details.Description;
                await SetProjectTile(tile, false);
            }

            NotifyStateChanged();
        }

        public async Task SetProjectTile(ProjectTileDto tile, bool notify = true)
        {
            if (tile is null) return;

            var tiles = await GetUserProjectTiles();
            if (tiles is null) return;

            var existingIndex = tiles.FindIndex(t => t.Id == tile.Id);
            if (existingIndex == -1)
                tiles.Add(tile); 
            
            else
                tiles[existingIndex] = tile;

            var options = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(20))
              .SetSize(1);

            _cache.Set(await GetTilesKey(), tiles, options);

            if (notify) NotifyStateChanged();
        }

        public async Task SetProjectTiles(List<ProjectTileDto> projects, bool notify = true)
        {
            if (projects is null) return;

            var options = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(20))
              .SetSize(1);

            var key = await GetTilesKey(); 

            _cache.Set(key, projects, options);

            if(notify) NotifyStateChanged();
        }

        public async Task SetAllProjectDetails(ProjectDetailedViewDto projectDetails, bool notify = true)
        {
            if (projectDetails is null || string.IsNullOrEmpty(projectDetails.Title))
                return; 

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20))
                .SetSize(1);

            var key = await GetDetailsKey(projectDetails.Id);

            _cache.Set(key, projectDetails, options);

            if (notify) NotifyStateChanged();
        }

        public async Task SetTodoItemInProject(Guid projectId, TodoItemEntry todoItem)
        {
            if (projectId == Guid.Empty ||  todoItem == null) return;
            bool isNewTask = false;

            //Project Details
            var cachedProject = await GetProjectDetails(projectId);
            if (cachedProject == null) return; 

            var index = cachedProject.TodoItems.FindIndex(t => t.Id == todoItem.Id);
            if (index != -1)
            {
                cachedProject.TodoItems[index] = todoItem;
            }
            else
            {
                cachedProject.TodoItems.Add(todoItem);
                cachedProject.TotalTodoItemCount++;
                isNewTask = true;
            }

            //Tile
            if (isNewTask)
            {
                var tile = await GetProjectTile(projectId);
                if (tile is not null)
                {
                    tile.TotalTodoItemCount++;
                    await SetProjectTile(tile, false);
                }
            }

            await SetAllProjectDetails(cachedProject);
            NotifyStateChanged();
        }


        //----------- Removals ----------- 
        public async Task RemoveProject(Guid projectId)
        {
            if (projectId == Guid.Empty) return;

            _cache.Remove(await GetDetailsKey(projectId));

            var tiles = await GetUserProjectTiles();
            if (tiles is not null)
            {
                var index = tiles.FindIndex(p => p.Id == projectId);

                if (index != -1)
                {
                    tiles.RemoveAt(index);
                    await SetProjectTiles(tiles, false);
                }
            }

            NotifyStateChanged();
        }
        public async Task RemoveTodoItem(Guid projectId, Guid todoItemId)
        {
            if (projectId == Guid.Empty || todoItemId == Guid.Empty) return;

            var project = await GetProjectDetails(projectId);
            if (project is null || project.TodoItems is null) return;

            var todoItem = project.TodoItems.FirstOrDefault(t => t.Id == todoItemId);
            if (todoItem is not null)
            {
                var isComplete = todoItem.Status == Domain.Enums.Status.Complete;
                project.TotalTodoItemCount--;

                if (isComplete)
                    project.CompleteTodoItemCount--;

                project.TodoItems.Remove(todoItem);
                await SetAllProjectDetails(project, false);

                var tiles = await GetUserProjectTiles();
                if (tiles is null) return;

                var index = tiles.FindIndex(p => p.Id == projectId);
                if (index != -1)
                {
                    var newTile = project.ToProjectTileDto();
                    if (newTile is not null)
                        await SetProjectTile(newTile, false);
                }

                NotifyStateChanged(); 
            }
        }

        public async Task UpdateTodoItemStatus(Guid projectId, Guid todoItemId)
        {
            if (projectId == Guid.Empty || todoItemId == Guid.Empty) return;

            var project = await GetProjectDetails(projectId);
            if (project is null || project.TodoItems is null) return;

            var todoItem = project.TodoItems.FirstOrDefault(t => t.Id == todoItemId);
            if (todoItem is null) return;

            var wasComplete = todoItem.Status == Domain.Enums.Status.Complete;
            if (wasComplete)
            {
                project.CompleteTodoItemCount--;
                todoItem.Status = Domain.Enums.Status.Incomplete; 
            }

            else
            {
                project.CompleteTodoItemCount++;
                todoItem.Status = Domain.Enums.Status.Complete;
            }

            await SetAllProjectDetails(project, false);
            
            //Handle Tile
            var tiles = await GetUserProjectTiles();
            if (tiles is null) return;

            var tileToUpdate = tiles.FirstOrDefault(p => p.Id == projectId);
            if (tileToUpdate is null) return;

            if (wasComplete) 
                tileToUpdate.CompleteTodoItemCount--; 

            else
                tileToUpdate.CompleteTodoItemCount++;

            await SetProjectTiles(tiles, false);
            NotifyStateChanged(); 
        }


        public ProjectDetailedViewDto? Clone(ProjectDetailedViewDto original)
        {
            if (original is null) return null;

            return new ProjectDetailedViewDto
            {
                Id = original.Id,
                Title = original.Title,
                Description = original.Description,
                TotalTodoItemCount = original.TotalTodoItemCount,
                CompleteTodoItemCount = original.CompleteTodoItemCount,
                Status = original.Status,
                CreatedOn = original.CreatedOn,
                TodoItems = original.TodoItems.Select(t => new TodoItemEntry
                {
                    Id = t.Id,
                    AssigneeId = t.AssigneeId,
                    OwnerId = t.OwnerId,
                    Title = t.Title,
                    Description = t.Description,
                    ProjectTitle = t.ProjectTitle,
                    Priority = t.Priority,
                    AssigneeName = t.AssigneeName,
                    OwnerName = t.OwnerName,
                    DueDate = t.DueDate,
                    CreatedOn = t.CreatedOn,
                    Status = t.Status
                }).ToList()
            }; 
        }

        public ProjectTileDto? Clone(ProjectTileDto original)
        {
            if (original is null) return null;

            return new ProjectTileDto
            {
                Id = original.Id,
                OwnerId = original.OwnerId,
                Title = original.Title,
                Description = original.Description,
                TotalTodoItemCount = original.TotalTodoItemCount,
                CompleteTodoItemCount = original.CompleteTodoItemCount,
                CreatedOn = original.CreatedOn,
                Status = original.Status
            };
        }

        public List<ProjectTileDto>? Clone(List<ProjectTileDto> original)
        {
            if (original is null) return null;

            return original.Select(t => new ProjectTileDto 
            {
                Id = t.Id,
                OwnerId = t.OwnerId,
                Title = t.Title,
                Description = t.Description,
                TotalTodoItemCount = t.TotalTodoItemCount,
                CompleteTodoItemCount = t.CompleteTodoItemCount,
                CreatedOn = t.CreatedOn,
                Status = t.Status
            }).ToList();
        }

        public TodoItemEntry? Clone(TodoItemEntry original)
        {
            if (original is null) return null;

            return new TodoItemEntry
            {
                Id = original.Id,
                AssigneeId = original.AssigneeId,
                OwnerId = original.OwnerId,
                Title = original.Title,
                Description = original.Description,
                ProjectTitle = original.ProjectTitle,
                AssigneeName = original.AssigneeName,
                OwnerName = original.OwnerName,
                DueDate = original.DueDate,
                CreatedOn = original.CreatedOn,
                Status = original.Status
            };
        }

        public async Task ClearProjectTiles()
        {
            _cache.Remove(await GetTilesKey());
            NotifyStateChanged();
        }

        public async Task ClearProjectDetails(Guid projectId)
        {
            _cache.Remove(await GetDetailsKey(projectId));
            NotifyStateChanged();

        }
        public async Task ClearUserCache()
        {
            try
            {
                var tiles = await GetUserProjectTiles();
                if (tiles is null || tiles.Count == 0) return;

                foreach (var tile in tiles)
                {
                    _cache.Remove(await GetDetailsKey(tile.Id));
                }

                _cache.Remove(await GetTilesKey());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis Cache Cleanup Error: {ex}");
            }

            NotifyStateChanged();
        }
    }
}
