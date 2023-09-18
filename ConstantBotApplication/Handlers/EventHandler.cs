using ConstantBotApplication.Domain;
using ConstantBotApplication.Modules.Events.Abstractions;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Handlers;

public class EventHandler
{
    private readonly DiscordClient client;
    private readonly IServiceProvider services;

    public EventHandler(DiscordClient client, IServiceProvider services)
    {
        this.client = client;
        this.services = services;
    }

    public void RegisterEvents()
    {
        var modules = services.GetServices<IEventModule>();
        foreach (var module in modules)
            module.Register(client);
    }
}
