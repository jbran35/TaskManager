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
    public class GetProjectDetailedViewQueryHandler(IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<GetProjectDetailedViewQueryHandler> logger) : IRequestHandler<GetProjectDetailedViewQuery, Result<ProjectDetailedViewDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<GetProjectDetailedViewQueryHandler> _logger = logger;
        public async Task<Result<ProjectDetailedViewDto>> Handle(GetProjectDetailedViewQuery request, CancellationToken cancellationToken)
        {
            //Check Cache
            string key = CacheKeys.ProjectDetailedViews(request.UserId, request.ProjectId);

            try
            {
                _logger.LogInformation("Trying to get Project Details from Redis");
                var cachedProjectJson = await _cache.GetStringAsync(key, cancellationToken);

                if (!string.IsNullOrEmpty(cachedProjectJson))
                {
                    _logger.LogInformation("Pulling Project Details from Redis");
                    var cachedProject = JsonSerializer.Deserialize<ProjectDetailedViewDto>(cachedProjectJson);

                    if (cachedProject is not null)
                        return Result<ProjectDetailedViewDto>.Success(cachedProject);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Error:");
            }

            _logger.LogInformation("Getting Project Details From Database");

            //Validate project
            var project = await _unitOfWork.ProjectRepository.GetProjectDetailedViewAsync(request.ProjectId, cancellationToken);

            if (project is null || project.OwnerId != request.UserId)
                return Result<ProjectDetailedViewDto>.Failure("Project Not Found");

            //Map project to DTO & Return
            var projectDetailedViewDto = project.ToProjectDetailedViewDto();


            try
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(20),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _logger.LogInformation("Saving Project Details To Redis");

                string serializedDto = JsonSerializer.Serialize(projectDetailedViewDto, jsonOptions);
                await _cache.SetStringAsync(key, serializedDto, cacheOptions, cancellationToken);

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Error:");
            }


            return Result<ProjectDetailedViewDto>.Success(projectDetailedViewDto!);
        }
    }
}
