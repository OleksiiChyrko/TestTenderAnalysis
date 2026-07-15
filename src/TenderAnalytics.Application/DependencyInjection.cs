using Microsoft.Extensions.DependencyInjection;
using TenderAnalytics.Application.Interfaces.Mapping;
using TenderAnalytics.Application.Mapping;

namespace TenderAnalytics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<ITenderMapper, TenderMapper>();

        return services;
    }
}