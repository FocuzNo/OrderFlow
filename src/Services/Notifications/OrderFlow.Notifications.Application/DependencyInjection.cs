using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Notifications.Application.Behaviors;
namespace OrderFlow.Notifications.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddMediatR(c => { c.RegisterServicesFromAssembly(assembly); c.AddOpenBehavior(typeof(ValidationBehavior<,>)); c.AddOpenBehavior(typeof(LoggingBehavior<,>)); });
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
}
