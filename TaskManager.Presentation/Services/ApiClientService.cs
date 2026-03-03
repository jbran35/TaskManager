namespace TaskManager.Presentation.Services
{
    public class ApiClientService
    {
        private readonly HttpClient _httpClient;

        // Note: No TokenProviderService here! 
        // The HttpClient coming in is already "pre-loaded" with the AuthHeaderHandler
        public ApiClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient GetClient()
        {
            // We don't need to manually set headers here anymore.
            // The AuthHeaderHandler (in the background) will intercept the call
            // and slap the token on right before it leaves the building.
            return _httpClient;
        }
    }
}