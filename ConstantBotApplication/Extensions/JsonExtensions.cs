using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConstantBotApplication.Extensions;

public static class JsonExtensions
{
    public static IServiceCollection AddDefaultJsonOptions(this IServiceCollection services)
    {
        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        jsonOptions.WriteIndented = true;

        services.AddSingleton(jsonOptions);

        return services;
    }
}
