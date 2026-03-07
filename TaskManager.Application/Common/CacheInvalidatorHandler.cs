using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Common
{
    public class CacheInvalidatorHandler<TRequest, TResponse>(IDistributedCache cache, ILogger<CacheInvalidatorHandler<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<CacheInvalidatorHandler<TRequest, TResponse>> _logger = logger; 
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("In Handle Method"); 
            var response = await next();

            if (request is ICacheInvalidator cacheInvalidator && response is Result { IsSuccess: true})
            {
                _logger.LogInformation("In Handle Method - If Block");


                foreach (var key in cacheInvalidator.Keys)
                {
                    try
                    {
                        _logger.LogInformation("In Handle Method - For Loop. Key: " + key);
                        await _cache.RemoveAsync(key, CancellationToken.None);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "Issue Clearing Assignee's Cached Task List");
                    }
                }
            }

            _logger.LogInformation("In Handle Method - Returning");

            return response; 
        }
    }
}
