using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Registra por convenção todas as classes de feature terminadas em "Service".
        var serviceTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Service"));

        foreach (var type in serviceTypes)
            services.AddScoped(type);

        // Registra todos os validators (FluentValidation) do assembly.
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
