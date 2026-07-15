using Microsoft.Extensions.DependencyInjection;
using TenderAnalytics.Application.Interfaces.Mapping;
using TenderAnalytics.Application.Interfaces.Services;
using TenderAnalytics.Application.Mapping;
using TenderAnalytics.Application.Services;

namespace TenderAnalytics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<ITenderMapper, TenderMapper>();

        services.AddScoped<ITenderImportService, TenderImportService>();

        return services;
    }
}