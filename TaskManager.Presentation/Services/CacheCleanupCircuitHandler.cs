using Microsoft.AspNetCore.Components.Server.Circuits;

namespace TaskManager.Presentation.Services
{
    public class CacheCleanupCircuitHandler(ProjectStateService cacheService) : CircuitHandler
    {
        public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            await cacheService.ClearUserCache();
            await base.OnCircuitClosedAsync(circuit, cancellationToken);
        }
    }
}
