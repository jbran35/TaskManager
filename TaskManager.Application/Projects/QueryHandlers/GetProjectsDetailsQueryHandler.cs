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
    public class GetProjectsDetailsQueryHandler(IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<GetProjectsDetailsQueryHandler> logger) : IRequestHandler<GetProjectDetailsQuery, Result<ProjectDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<GetProjectsDetailsQueryHandler> _logger = logger;
        public async Task<Result<ProjectDetailsDto>> Handle(GetProjectDetailsQuery request, CancellationToken cancellationToken)
        {
            //Validate Request
            if(request is null || request.ProjectId == Guid.Empty || request.UserId == Guid.Empty)
                return Result<ProjectDetailsDto>.Failure("Invalid Request");

            //Check Cache For Project
            _logger.LogInformation("Checking For Project Details In Cache"); 

            string key = CacheKeys.ProjectDetailedViews(request.UserId, request.ProjectId);

            try
            {
                var cachedProject = await _cache.GetStringAsync(key, cancellationToken);

                if (!string.IsNullOrEmpty(cachedProject))
                {
                    _logger.LogInformation("Pulling Project Details From Redis Cache...");

                    var proj = JsonSerializer.Deserialize<ProjectDetailedViewDto>(cachedProject);

                    if (proj is not null)
                    {
                        return Result<ProjectDetailsDto>.Success(new ProjectDetailsDto
                        {
                            Id = proj.Id,
                            Title = proj.Title,
                            Description = proj.Description,
                            CreatedOn = proj.CreatedOn
                        });  
                    }
                }
            }

            catch(Exception ex)
            {
                _logger.LogError(ex, "Error reading from Redis cache.");
            }

            //Validate Project
            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);

            if (project is null || project.OwnerId != request.UserId)
                return Result<ProjectDetailsDto>.Failure("Project Not Found");

            //Map to Dto and return
            var detailsDto = project.ToProjectDetailsDto();
            return Result<ProjectDetailsDto>.Success(detailsDto!);

        }
    }
}
