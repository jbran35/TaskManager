using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaskManager.Application.Common;

namespace TaskManager.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

                cfg.AddOpenBehavior(typeof(CacheInvalidatorHandler<,>));

            });

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;

        }
    }
}
