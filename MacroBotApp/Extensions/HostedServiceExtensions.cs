using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MacroBot.Extensions;

public static class HostedServiceExtensions
{
    public static IServiceCollection AddInjectableHostedService<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, IHostedService, TService
    {
        services
            .AddSingleton<TImplementation>()
            .AddSingleton<IHostedService>(provider => provider.GetRequiredService<TImplementation>())
            .AddSingleton<TService>(provider => provider.GetRequiredService<TImplementation>());
        return services;
    }
    
}