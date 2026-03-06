using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TaskManager.Domain.Entities;

namespace TaskManager.Presentation.Services
{
    public class SignalRConnectionService(AuthenticationStateProvider authStateProvider)
    {
        private HubConnection? _hubConnection;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
        public event Func<Task>? OnTaskUpdated;

        public HubConnectionState HubState => _hubConnection?.State ?? HubConnectionState.Disconnected;
        public bool IsConnected => HubState == HubConnectionState.Connected;

        public async Task<string?> GetTokenAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            //Console.WriteLine(user.FindFirst("jwt_token")?.Value);
            return user.FindFirst("jwt_token")?.Value;
        }
        public async Task InitializeConnection()
        {
            _hubConnection = new HubConnectionBuilder()

            .WithUrl(("https://localhost:7109/taskhub"), options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    return await GetTokenAsync();
                };
            })
            .ConfigureLogging(logging => {
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddConsole();
            })
            .WithAutomaticReconnect()

            .Build();

            _hubConnection.On("TaskUpdated", async () =>
            {
                Console.WriteLine("In OnTaskUpdated");
                OnTaskUpdated?.Invoke();
            });

            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("Connection Started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting: {ex.Message}");
            }
        }

        

    }
}
