using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskManager.API.Hubs
{

    [Authorize]
    public class TaskHub : Hub
    {
        public async Task SendUpdateNotification(string user)
        {
            Console.WriteLine("SENDING FROM API AUTHORIZED");
            await Clients.User(user).SendAsync("TaskUpdated");
        }
    }
}
