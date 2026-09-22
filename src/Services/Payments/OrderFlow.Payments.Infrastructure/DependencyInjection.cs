using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Infrastructure.Payments;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("PaymentsDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:PaymentsDatabase is required."
            );

        services.Configure<DevelopmentPaymentGatewayOptions>(
            configuration.GetSection(DevelopmentPaymentGatewayOptions.SectionName)
        );
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<PaymentsDbContext>()
        );
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddSingleton<IPaymentGateway, DevelopmentPaymentGateway>();

        return services;
    }
}
