using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Mvc;
using Polly;
using System.Security.Claims;
using System.Text.Json;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Users.DTOs.Requests;
using TaskManager.Application.Users.DTOs.Responses;
using TaskManager.Presentation.Components;
using TaskManager.Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();


builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddScoped<AssignedTodoItemsStateService>();
builder.Services.AddScoped<ProjectStateService>();
builder.Services.AddScoped<AssigneeListStateService>();
builder.Services.AddScoped<ProjectSortStateService>(); 
builder.Services.AddScoped<TodoItemDraftStateService>();
builder.Services.AddScoped<TokenProviderService>();
builder.Services.AddScoped<SignalRConnectionService>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CircuitHandler, CacheCleanupCircuitHandler>();



builder.Services.AddHttpClient("PublicAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7109");
});


builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7109");
    //});

}).AddHttpMessageHandler<AuthHeaderHandler>()
.AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(7)));
//.SetHandlerLifetime(TimeSpan.FromSeconds(1)); 



builder.Services.AddScoped<ApiClientService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var authProvider = sp.GetRequiredService<AuthenticationStateProvider>();

    var authHandler = new AuthHeaderHandler(authProvider);

    authHandler.InnerHandler = new HttpClientHandler();

    var baseClient = factory.CreateClient("API");

    var authenticatedClient = new HttpClient(authHandler)
    {
        BaseAddress = baseClient.BaseAddress
    };

    return new ApiClientService(authenticatedClient);
});


builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {

        options.LoginPath = "/login";
        options.Cookie.Name = "BlazorAuth";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;

    });


builder.Services.AddAuthorizationCore();
builder.Services.AddBlazorBootstrap();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.Use(async (context, next) =>
{
    Console.WriteLine(context.Request.Path);
    if (context.Request.Path.Equals("/", StringComparison.OrdinalIgnoreCase)
        && context.User?.Identity?.IsAuthenticated == true)
    {
        context.Response.Redirect("/myprojects");
        return;
    }
    await next();
});

app.MapPost("/account/login", async (HttpContext context, [FromForm] string username, [FromForm] string password, IHttpClientFactory clientFactory) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    var client = clientFactory.CreateClient("PublicAPI");


    var response = await client.PostAsJsonAsync("api/account/login", new { Username = username, Password = password });

    if (!response.IsSuccessStatusCode)
    {
        return Results.Redirect("/login?error=InvalidCredentials");
    }

    var loginResponse = await response.Content.ReadFromJsonAsync<LoginUserResponse>();


    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, loginResponse?.UserName ?? string.Empty),
        new(ClaimTypes.Email,loginResponse?.Email ?? string.Empty),
        new(ClaimTypes.GivenName, loginResponse?.FirstName ?? string.Empty),
        new(ClaimTypes.Surname, loginResponse?.LastName ?? string.Empty),
        new(ClaimTypes.NameIdentifier, loginResponse?.Id.ToString() ?? string.Empty),
        new("jwt_token", loginResponse?.Token ?? string.Empty)
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
        });

    return Results.Redirect("/myprojects");
});

app.MapPost("/account/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    return Results.Redirect("/login");
});

app.MapPost("/account/refresh-identity", async (
    HttpContext context,
    [FromForm] string jsonPayload) =>
{

    string? returnUrl = context.Request.Headers["Referer"].ToString();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var request = JsonSerializer.Deserialize<UpdateProfileRequest>(jsonPayload, options);

    var identity = context.User.Identity as ClaimsIdentity;

    if (identity is null || identity.IsAuthenticated != true || request is null)
    {
        return Results.Redirect("/login");
    }

    var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var jwtToken = identity.FindFirst("jwt_token")?.Value;

    var existingClaims = new[] { ClaimTypes.GivenName, ClaimTypes.Surname, ClaimTypes.Name, ClaimTypes.Email };

    foreach (var type in existingClaims)
    {
        var claim = identity.FindFirst(type);

        if (claim is not null)
        {
            identity.RemoveClaim(claim);
        }

    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, request.UserName ?? string.Empty),
        new(ClaimTypes.Email, request.Email ?? string.Empty),
        new(ClaimTypes.GivenName, request.FirstName ?? string.Empty),
        new(ClaimTypes.Surname, request.LastName ?? string.Empty),
        new(ClaimTypes.NameIdentifier, userId ?? string.Empty),
        new("jwt_token", jwtToken ?? string.Empty)
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
        });

    return Results.Redirect(returnUrl);
});

app.MapStaticAssets();



app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();



app.Run();