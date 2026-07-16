using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using TenderAnalytics.Application.Interfaces.External;
using TenderAnalytics.Application.Interfaces.Repositories;
using TenderAnalytics.Application.Interfaces.Services;
using TenderAnalytics.Infrastructure.Analytics;
using TenderAnalytics.Infrastructure.External.Clients;
using TenderAnalytics.Infrastructure.Persistence;
using TenderAnalytics.Infrastructure.Persistence.Repositories;

namespace TenderAnalytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException(
                "Connection string 'PostgreSql' was not found.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ITenderRepository, TenderRepository>();
        services
            .AddHttpClient<ITenderApiClient, TenderApiClient>(client =>
            {
                client.BaseAddress = new Uri(
                    "https://public-api.prozorro.gov.ua/api/2.5/");

                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.Retry.BackoffType =
                    DelayBackoffType.Exponential;

                options.AttemptTimeout.Timeout =
                    TimeSpan.FromSeconds(15);

                options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(45);
            });

        return services;
    }
}