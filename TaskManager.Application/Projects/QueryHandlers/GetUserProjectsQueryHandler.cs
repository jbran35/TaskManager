using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskManager.Application.Common;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.Mappers;
using TaskManager.Application.Projects.Queries;
using TaskManager.Domain.Common;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.QueryHandlers
{
    public class GetUserProjectsQueryHandler(IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<GetUserProjectsQueryHandler> logger) : IRequestHandler<GetUserProjectsQuery, Result<List<ProjectTileDto>>>
    {
        private readonly IUnitOfWork unitOfWork = unitOfWork;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<GetUserProjectsQueryHandler> _logger = logger;
        public async Task<Result<List<ProjectTileDto>>> Handle(GetUserProjectsQuery request, CancellationToken cancellationToken)
        {
            string key = CacheKeys.ProjectTiles(request.UserId);

            try
            {
                _logger.LogInformation("Trying to get Project Tiles from Cache");
                var cachedTiles = await _cache.GetStringAsync(key, cancellationToken);

                if (!string.IsNullOrEmpty(cachedTiles))
                { 
                    _logger.LogInformation("Pulling Project Tiles From Redis Cache...");

                    var tiles = JsonSerializer.Deserialize<List<ProjectTileDto>>(cachedTiles);

                    return Result<List<ProjectTileDto>>.Success(tiles!);
                }
            }

            catch(Exception ex)
            {
                _logger.LogError(ex, "Redis Error:");
            }

            _logger.LogInformation("No Project Tiles found in cache. Pulling from the Repository...");

            var readOnlyList = await unitOfWork.ProjectRepository
                .GetAllProjectsByOwnerIdAsync(request.UserId, cancellationToken);

            if (readOnlyList is null)
                return Result<List<ProjectTileDto>>.Failure("Issue Retrieving Projects");

            var projectTiles = readOnlyList.ToProjectTileDtoList();
            //var projectTiles = readOnlyList.Cast<ProjectTileDto>().ToList(); 

            try
            {
                _logger.LogInformation("Saving Project Tiles to Cache");

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(20),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                string projectTilesJson = JsonSerializer.Serialize(projectTiles, jsonOptions);
                await _cache.SetStringAsync(key, projectTilesJson, cacheOptions, cancellationToken);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Error:");
            }

            return Result<List<ProjectTileDto>>.Success(projectTiles!, "Projects Retrieved Successfully");
        }
    }
}
