using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra por convenção todas as classes de feature terminadas em "Service".
        var assembly = Assembly.GetExecutingAssembly();
        var serviceTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Service"));

        foreach (var type in serviceTypes)
            services.AddScoped(type);

        return services;
    }
}
