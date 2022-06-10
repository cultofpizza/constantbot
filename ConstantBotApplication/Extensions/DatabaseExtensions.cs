using ConstantBotApplication.Domain;
using Discord.WebSocket;
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
        var client = services.GetService<DiscordSocketClient>();

        context.SavedChanges += (e,ee) => context.ChangeTracker.AcceptAllChanges();

        client.JoinedGuild += async guild => await context.EnsureSettingsCreatedAsync(guild);

        foreach (var guild in client.Guilds)
        {
            await context.EnsureSettingsCreatedAsync(guild);
        }
        

        return services;
    }

    static async Task EnsureSettingsCreatedAsync(this BotContext context, SocketGuild guild)
    {
        if (0 == await context.GuildSettings.Where(i => i.GuilId == guild.Id).CountAsync())
        {
            await context.GuildSettings.AddAsync(new GuildSettings()
            {
                GuilId = guild.Id,
                MonitorChannelId = null,
                MonitoringEnable = false
            });
            await context.SaveChangesAsync();
        }
    }
}
