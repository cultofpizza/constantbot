using ConstantBotApplication.Domain;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddBotContext(this IServiceCollection services)
    { 
        services.AddDbContext<BotContext>(builder =>
        {
            builder.UseNpgsql(Environment.GetEnvironmentVariable("connectionString"));
        },ServiceLifetime.Singleton);


        return services;
    }

    public static async Task<IServiceProvider> InitializeBotContextAsync(this IServiceProvider services)
    {
        var context = services.GetService<BotContext>();
        var client = services.GetService<DiscordClient>();

        context.SavedChanges += (e,ee) => context.ChangeTracker.AcceptAllChanges();

        client.GuildCreated += async (client, args) => await context.EnsureSettingsCreatedAsync(args.Guild);

        foreach (var guild in client.Guilds)
        {
            await context.EnsureSettingsCreatedAsync(guild.Value);
        }
        

        return services;
    }

    static async Task EnsureSettingsCreatedAsync(this BotContext context, DiscordGuild guild)
    {
        if (0 == await context.Guilds.Where(i => i.GuildId == guild.Id).CountAsync())
        {
            await context.Guilds.AddAsync(new Guild()
            {
                GuildId = guild.Id,
                MonitorChannelId = null
            });
            await context.SaveChangesAsync();
        }
    }
}
