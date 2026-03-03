using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.TodoItems.DTOs;
namespace TaskManager.Presentation.Services
{
    public class ProjectStateService(IMemoryCache cache, AuthenticationStateProvider authStateProvider)
    {

        private readonly IMemoryCache _cache = cache;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

        private async Task<string> GetUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        }

        private async Task<string> GetDetailsKey(Guid projectId) => $"{await GetUserIdAsync()}_project_details_{projectId}";
        private async Task<string> GetTilesKey() => $"project_tiles_{await GetUserIdAsync()}";

        //----------- Getters ----------- 

        public async Task<ProjectDetailedViewDto?> GetProjectDetails(Guid projectId)
        {
            var original = _cache.Get<ProjectDetailedViewDto>(await GetDetailsKey(projectId));
            return original is not null ? await Clone(original) : null;
        }

        public async Task<List<ProjectTileDto>?> GetUserProjectTiles()
        {
            var original = _cache.Get<List<ProjectTileDto>>(await GetTilesKey());
            return original is not null ? await Clone(original) : null;

        }

        public async Task<TodoItemEntry?> GetTodoItem(Guid projectId, Guid todoItemId)
        {
            var original = _cache.Get<ProjectDetailedViewDto>(await GetDetailsKey(projectId))?
                .TodoItems
                .FirstOrDefault(t => t.Id == todoItemId);

            return original is not null ? await Clone(original) : null;
        }

        public async Task<ProjectTileDto?> GetProjectTile(Guid projectId)
        {
            var original = _cache
                .Get<List<ProjectTileDto>>(await GetTilesKey())?
                .FirstOrDefault(p => p.Id == projectId);

            return original is not null ? await Clone(original) : null;
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

            return (await Clone(original))?.TodoItems ?? null; 
        }


        //----------- Setters ----------- 
        public async Task SetProjectBasicDetails(ProjectDetailsDto details)
        {
            if (details is null) return;

            var project = await GetProjectDetails(details.Id);

            var options = new MemoryCacheEntryOptions()
             .SetSlidingExpiration(TimeSpan.FromMinutes(20))
             .SetSize(1);

            if (project is null)
            {
                var projDetails = new ProjectDetailedViewDto
                {
                    Id = details.Id,
                    Title = details.Title,
                    Description = details.Description,
                    CreatedOn = details.CreatedOn,
                };

                _cache.Set(await GetDetailsKey(projDetails.Id), projDetails, options);
                _cache.Remove(await GetTilesKey()); 
            }

            else
            {
                //Id, Title, Description, CreatedOn
                project.Title = details.Title;
                project.Description = details.Description;
                _cache.Set(await GetDetailsKey(project.Id), project, options);
                _cache.Remove(await GetTilesKey());
            }

            return;
        }

        public async Task SetProjectTile(ProjectTileDto tile)
        {
            if (tile is null) return;

            var tiles = await GetUserProjectTiles();

            if (tiles is null) return;

            var neededTile = tiles.FirstOrDefault(t => t.Id == tile.Id); 

            //Set new
            if (neededTile is null)
            {
                tiles.Add(tile);
            }

            //Overwrite
            else
            {
                neededTile = tile; 
            }

            var options = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(20))
              .SetSize(1);

            _cache.Set(await GetTilesKey(), tiles, options);
        }

        public async Task SetProjectTiles(List<ProjectTileDto> projects)
        {
            Console.WriteLine("\n SETTING PROJECT TILES IN CACHE \n");
            if (projects is null) return;

            var options = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(20))
              .SetSize(1);

            var key = await GetTilesKey(); 

            _cache.Set(key, projects, options);

            Console.WriteLine("Set in cache");
        }

        public async Task SetAllProjectDetails(ProjectDetailedViewDto projectDetails)
        {
            if (projectDetails is null || string.IsNullOrEmpty(projectDetails.Title))
                return; 

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20))
                .SetSize(1);

            var key = await GetDetailsKey(projectDetails.Id);
            Console.WriteLine("In SetAllProjectDetails, key used: " + key);

            _cache.Set(key, projectDetails, options);
        }

        public async Task SetTodoItemInProject(Guid projectId, TodoItemEntry todoItem)
        {
            if (projectId == Guid.Empty ||  todoItem == null)
                return;


            var project = await GetProjectDetails(projectId);

            if (project == null)
                return; 

            var existingIndex = project.TodoItems.FindIndex(t => t.Id == todoItem.Id);

            if (existingIndex != -1)
            {
                project.TodoItems[existingIndex] = todoItem;
            }
            else
            {
                project.TodoItems.Add(todoItem);
                project.TotalTodoItemCount++; 
            }


            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                .SetSize(1);

            string detailsKey = await GetDetailsKey(projectId);

            string tileKey = await GetTilesKey(); 

            _cache.Set(await GetDetailsKey(projectId), project, options);
            _cache.Remove(tileKey); 
        }

        public async Task RemoveProject(Guid projectId)
        {
            if (projectId == Guid.Empty) return;

            _cache.Remove(await GetDetailsKey(projectId));

            var tiles = await GetUserProjectTiles();

            if (tiles is not null)
            {
                var tileToRemove = tiles.FirstOrDefault(p => p.Id == projectId);

                if (tileToRemove is not null)
                {
                    tiles.Remove(tileToRemove);
                    await SetProjectTiles(tiles);
                }
            }
        }
        
        public async Task RemoveTodoItem(Guid projectId, Guid todoItemId)
        {
            if (projectId == Guid.Empty || todoItemId == Guid.Empty) return;

            var project = await GetProjectDetails(projectId);

            if (project is null || project.TodoItems is null) return;

            var todoItem = project.TodoItems.FirstOrDefault(t => t.Id == todoItemId);

            var isComplete = false;

            if (todoItem is not null)
            {
                //Handle Project Details
                project.TotalTodoItemCount--;

                if (todoItem.Status == Domain.Enums.Status.Complete)
                {
                    project.CompleteTodoItemCount--;
                    isComplete = true;
                }

                project.TodoItems.Remove(todoItem);
                await SetAllProjectDetails(project);

                //Handle Project Tile
                var tiles = await GetUserProjectTiles();

                if (tiles is null) return;

                var tileToUpdate = tiles.FirstOrDefault(p => p.Id == projectId);

                if (tileToUpdate is null) return; 

                tileToUpdate.TotalTodoItemCount--;

                if (isComplete)
                    tileToUpdate.CompleteTodoItemCount--;

                await SetProjectTiles(tiles); 
            }
        }

        public async Task UpdateTodoItemStatus(Guid projectId, Guid todoItemId)
        {
            if (projectId == Guid.Empty || todoItemId == Guid.Empty) return;

            var project = await GetProjectDetails(projectId);
            if (project is null || project.TodoItems is null) return;

            var todoItem = project.TodoItems.FirstOrDefault(t => t.Id == todoItemId);
            if (todoItem is null) return;

            //Discern which way we're flipping the status
            var wasComplete = todoItem.Status == Domain.Enums.Status.Complete;


            //If complete > Change to incomplete
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

            await SetAllProjectDetails(project);
            
            //Handle Tile
            var tiles = await GetUserProjectTiles();

            if (tiles is null) return;

            var tileToUpdate = tiles.FirstOrDefault(p => p.Id == projectId);

            if (tileToUpdate is null) return;

            if (wasComplete) { tileToUpdate.CompleteTodoItemCount--; }

            else { tileToUpdate.CompleteTodoItemCount++; }

            await SetProjectTiles(tiles);
        }


        public async Task<ProjectDetailedViewDto?> Clone(ProjectDetailedViewDto original)
        {
            if (original is null) return null;

            //Id, Title, Description, TotalTodoItemsCount, CompleteTodoItemsCount, Status, CreatedOn, TodoItems

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
                    //Id, AssigneeId, OwnerId, Title, Description, ProjectTitle, AssigneeName,
                    //OwnerName, Priority, DueDate, CreatedOn, Status

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

        public async Task<ProjectTileDto?> Clone(ProjectTileDto original)
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

        public async Task<List<ProjectTileDto>?> Clone(List<ProjectTileDto> original)
        {
            if (original is null) return null;

            //Id, OwnerId, Title, Description, TotalTodoItemCount, CompleteTodoItemCount, CreatedOn, Status


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

        public async Task<TodoItemEntry?> Clone(TodoItemEntry original)
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
        }

        public async Task ClearProjectDetails(Guid projectId)
        {
            _cache.Remove(await GetDetailsKey(projectId)); 
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
        }
    }
}
