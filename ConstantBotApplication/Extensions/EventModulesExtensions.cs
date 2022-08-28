using ConstantBotApplication.Modules.Events.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Extensions;

public static class EventModulesExtensions
{
    public static IServiceCollection AddEventModules(this IServiceCollection services)
    {
        Assembly.GetEntryAssembly().ExportedTypes
          .Where(x => typeof(IEventModule).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
          .ToList()
          .ForEach(i => services.AddSingleton(typeof(IEventModule),i));

        return services;
    }
}
