//namespace TaskManager.Presentation
//{
//    public class CookieHandler : DelegatingHandler
//    {
//        private readonly IHttpContextAccessor _httpContextAccessor;

//        public CookieHandler(IHttpContextAccessor httpContextAccessor)
//        {
//            _httpContextAccessor = httpContextAccessor;
//        }

//        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            // Grab the "BlazorAuth" cookie from the current browser session
//            var cookie = _httpContextAccessor.HttpContext?.Request.Cookies["BlazorAuth"];

//            if (!string.IsNullOrEmpty(cookie))
//            {
//                // Manually inject it into the internal HttpClient request
//                request.Headers.Add("Cookie", $"BlazorAuth={cookie}");
//            }

//            return base.SendAsync(request, cancellationToken);
//        }
//    }
//}
