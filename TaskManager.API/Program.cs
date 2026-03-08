using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using TaskManager.API.Hubs;
using TaskManager.API.Services;
using TaskManager.Application;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Seed;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

//Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Add Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<User>>();
;

//builder.Services.AddMediatR(cfg => {
//    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);

//    cfg.AddOpenBehavior(typeof(CacheInvalidatorHandler<,>));
//});


//Register Services
builder.Services.AddOpenApi("v1");
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
//builder.Services.AddScoped<IRegisterUserHandler, RegisterUserHandler>();
builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
builder.Services.AddScoped<IUserConnectionRepository, UserConnectionRepository>();
builder.Services.AddScoped<ITodoItemUpdateNotificationService, TodoItemUpdateNotificationService>(); 
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddApplication(); 

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (string.IsNullOrEmpty(redisConnection))
{
    throw new Exception("Redis connection string is missing!");
}

builder.Services.AddStackExchangeRedisCache(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    options.Configuration = connectionString;
    options.InstanceName = "TaskManager_";
});

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var presentationAppUrl = builder.Configuration.GetValue<string>("PresentationAppUrl");

if (string.IsNullOrWhiteSpace(presentationAppUrl))
{
    throw new InvalidOperationException("PresentationAppUrl configuration is missing or empty.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins(presentationAppUrl)
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/taskhub"))
            {
                Console.WriteLine("\n \n Made it to /taskhub condiditional for authentication \n \n");
                context.Token = accessToken;
            }

            else if (context.Request.Cookies.TryGetValue("authToken", out var token))
            {
                Console.WriteLine("\\n \n In cookie auth section \n \n");
                context.Token = token;
            }
          
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("Jwt:Key configuration is missing.")))
    };

    //options.Events = new JwtBearerEvents
    //{
    //    OnMessageReceived = context =>
    //    {
    //        context.Token = context.Request.Cookies["authToken"];
    //        return Task.CompletedTask;
    //    }
    //};
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowBlazorApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<TaskHub>("/taskhub");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    await DbInitializer.Seed(dbContext, userManager);
    Console.WriteLine("Database seeding completed.");
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // This simple check forces EF to compile the model and test the connection
    await context.Database.CanConnectAsync();

    // Optional: Execute a tiny query to warm up the Task repository logic
    _ = await context.Projects.AnyAsync();
}

app.Run();