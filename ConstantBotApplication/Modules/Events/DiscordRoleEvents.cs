using ConstantBotApplication.Domain;
using ConstantBotApplication.Modules.Events.Abstractions;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events;

public class DiscordRoleEvents : IDiscordRoleEventHandler
{
    private readonly BotContext _context;
    private readonly DiscordSocketClient _client;

    public DiscordRoleEvents(BotContext context, DiscordSocketClient client)
    {
        _context = context;
        _client = client;
    }

    public async Task RoleCreated(SocketRole role)
    {
        var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == role.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new EmbedBuilder()
                .WithAuthor(role.Guild.Name, role.Guild.IconUrl)
                .WithFooter($"ID: {role.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Green)
                .AddField("Color", role.Color)
                .WithDescription($"{Emoji.Parse(":military_medal:")} Created role ``{role.Name}``");
        if (role.Emoji.Name != null)
            builder.AddField("Emoji", role.Emoji);
        if (role.Icon != null)
            builder.AddField("New Icon", $"[Link]({role.GetIconUrl()})");
        if (role.Permissions.ToList().Count > 0)
            builder.AddField("Permissions", "None");
        else
        {
            var permissions = role.Permissions.ToList();
            string permissionsString = permissions[0].ToString();
            for (int i = 1; i < permissions.Count; i++)
            {
                permissionsString += ", " + permissions[i].ToString();
            }
            builder.AddField("Permissions", permissionsString);
        }

        var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
        await channel.SendMessageAsync(embed: builder.Build());
    }

    public async Task RoleDeleted(SocketRole role)
    {
        var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == role.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new EmbedBuilder()
                .WithAuthor(role.Guild.Name, role.Guild.IconUrl)
                .WithFooter($"ID: {role.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Red)
                .WithDescription($"{Emoji.Parse(":x:")} Deleted role ``{role.Name}``");

        var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
        await channel.SendMessageAsync(embed: builder.Build());
    }

    public async Task RoleUpdated(SocketRole roleBefore, SocketRole roleAfter)
    {
        var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == roleAfter.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new EmbedBuilder()
                .WithAuthor(roleAfter.Guild.Name, roleAfter.Guild.IconUrl)
                .WithFooter($"ID: {roleAfter.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Gold)
                .WithDescription($"{Emoji.Parse(":pencil2:")} Updated role ``{roleAfter.Name}``");

        if (roleBefore.Color != roleAfter.Color)
            builder.AddField("New color", roleAfter.Color);
        if (roleBefore.Emoji.Name != roleAfter.Emoji.Name)
            builder.AddField("New Emoji", roleAfter.Emoji);
        if (roleBefore.Icon != roleAfter.Icon)
            builder.AddField("New Icon", $"[Link]({roleAfter.GetIconUrl()})");
        if (!roleBefore.Permissions.Equals(roleAfter.Permissions))
        {
            var newPermissions = new List<string>();
            foreach (var item in roleAfter.Permissions.ToList())
            {
                if (!roleBefore.Permissions.Has(item))
                    newPermissions.Add(item.ToString());
            }

            if (newPermissions.Count > 0)
            {
                string permissionsString = newPermissions[0];
                for (int i = 1; i < newPermissions.Count; i++)
                {
                    permissionsString += ", " + newPermissions[i].ToString();
                }
                builder.AddField("Added permissions", permissionsString);
            }

            var deletedPermissions = new List<string>();
            foreach (var item in roleBefore.Permissions.ToList())
            {
                if (!roleAfter.Permissions.Has(item))
                    deletedPermissions.Add(item.ToString());
            }

            if (deletedPermissions.Count > 0)
            {
                string permissionsString = deletedPermissions[0];
                for (int i = 1; i < deletedPermissions.Count; i++)
                {
                    permissionsString += ", " + deletedPermissions[i].ToString();
                }
                builder.AddField("Deleted permissions", permissionsString);
            }

        }
        if (builder.Fields.Count == 0) return;

        var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
        await channel.SendMessageAsync(embed: builder.Build());
    }
}
