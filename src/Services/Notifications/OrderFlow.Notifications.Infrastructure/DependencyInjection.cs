using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Notifications.Application.Abstractions.Delivery;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Infrastructure.Delivery;
using OrderFlow.Notifications.Infrastructure.Persistence;

namespace OrderFlow.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("NotificationsDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:NotificationsDatabase is required."
            );

        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<NotificationsDbContext>()
        );
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();

        return services;
    }
}
