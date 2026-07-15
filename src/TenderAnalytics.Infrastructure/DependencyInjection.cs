using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TenderAnalytics.Infrastructure.Persistence;
using TenderAnalytics.Application.Interfaces.External;
using TenderAnalytics.Infrastructure.External.Clients;
using TenderAnalytics.Application.Interfaces.Repositories;
using TenderAnalytics.Infrastructure.Persistence.Repositories;
using TenderAnalytics.Application.Interfaces.Services;
using TenderAnalytics.Application.Services;

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

        services.AddScoped<ITenderRepository, TenderRepository>();
        services.AddScoped<ITenderImportService, TenderImportService>();

        services.AddHttpClient<ITenderApiClient, TenderApiClient>(client =>
        {
            client.BaseAddress = new Uri(
                "https://public-api.prozorro.gov.ua/api/2.5/");

            client.Timeout = TimeSpan.FromSeconds(30);
        });
        return services;
    }
}