namespace TaskManager.Presentation.Services
{
    public class ApiClientService
    {
        private readonly HttpClient _httpClient;

        public ApiClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient GetClient()
        {
            return _httpClient;
        }
    }
}