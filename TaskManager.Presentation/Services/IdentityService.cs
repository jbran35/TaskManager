using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Users.DTOs.Requests;

namespace TaskManager.Presentation.Services
{
    public class IdentityService (IHttpContextAccessor httpContextAccessor): IIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        public async Task RefreshUserClaimsAsync(UpdateProfileRequest request)
        {
            Console.WriteLine("\n \n IN IDENTITY SERVICE REFRESH USER CLAIMS \n");

            var context = _httpContextAccessor.HttpContext;

            if (context is null) return;

            var identity = context.User.Identity as ClaimsIdentity;
            if (identity is null) return;

            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var jwtToken = identity.FindFirst("jwt_token")?.Value;

             var claims = new List<Claim>
                {
                new(ClaimTypes.Name, request?.UserName ?? string.Empty),
                new(ClaimTypes.Email,request?.Email ?? string.Empty),
                new(ClaimTypes.GivenName, request?.FirstName ?? string.Empty),
                new(ClaimTypes.Surname, request?.LastName ?? string.Empty),
                new(ClaimTypes.NameIdentifier, request?.Id.ToString() ?? string.Empty),
                new("jwt_token", jwtToken ?? string.Empty)
                };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
            };

            await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),

            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
            });

            return; 

        }
    }
}
