using ConstantBotApplication.Domain;
using ConstantBotApplication.Modules.Events.Abstractions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events;

public class DiscordRoleEvents : IEventModule
{
    private readonly BotContext _context;
    private readonly DiscordClient _client;

    public DiscordRoleEvents(BotContext context, DiscordClient client)
    {
        _context = context;
        _client = client;
    }

    public void Register(DiscordClient client)
    {
        client.GuildRoleCreated += RoleCreated;
        client.GuildRoleDeleted += RoleDeleted;
        client.GuildRoleUpdated += RoleUpdated;
    }

    public async Task RoleCreated(DiscordClient client, GuildRoleCreateEventArgs args)
    {
        var role = args.Role;
        var guild = args.Guild;
        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.RolesMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new DiscordEmbedBuilder()
                .WithAuthor(guild.Name, null, guild.IconUrl)
                .WithFooter($"ID: {role.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Green)
                .AddField("Color", role.Color.ToString())
                .WithDescription($"{DiscordEmoji.FromName(client, ":military_medal:")} Created role ``{role.Name}``");
        if (role.Emoji != null)
            builder.AddField("Emoji", role.Emoji);
        if (role.IconUrl != null)
            builder.AddField("Icon", $"[Link]({role.IconUrl})");
        builder.AddField("Permissions", role.Permissions.ToPermissionString());

        var channel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
        await channel.SendMessageAsync(embed: builder.Build());
    }

    public async Task RoleDeleted(DiscordClient client, GuildRoleDeleteEventArgs args)
    {
        var role = args.Role;
        var guild = args.Guild;
        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.RolesMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new DiscordEmbedBuilder()
                .WithAuthor(guild.Name, null, guild.IconUrl)
                .WithFooter($"ID: {role.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Red)
                .WithDescription($"{DiscordEmoji.FromName(client, ":x:")} Deleted role ``{role.Name}``");

        var channel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
        await channel.SendMessageAsync(embed: builder.Build());
    }

    public async Task RoleUpdated(DiscordClient client, GuildRoleUpdateEventArgs args)
    {
        var roleBefore = args.RoleBefore;
        var roleAfter = args.RoleAfter;
        var guild = args.Guild;
        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.RolesMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new DiscordEmbedBuilder()
                .WithAuthor(guild.Name, null, guild.IconUrl)
                .WithFooter($"ID: {roleAfter.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Gold)
                .WithDescription($"{DiscordEmoji.FromName(client, ":pencil2:")} Updated role ``{roleAfter.Name}``");

        if (roleBefore.Color.Value != roleAfter.Color.Value)
            builder.AddField("New color", roleAfter.Color.ToString());
        if (roleBefore.Emoji != roleAfter.Emoji)
            builder.AddField("New Emoji", roleAfter.Emoji);
        if (roleBefore.IconUrl != roleAfter.IconUrl)
            builder.AddField("New Icon", $"[Link]({roleAfter.IconUrl})");
        if (!roleBefore.Permissions.Equals(roleAfter.Permissions))
        {
            var newPermissions = (Permissions)~(~(long)roleAfter.Permissions | (long)roleBefore.Permissions);
            builder.AddField("Added permissions", newPermissions.ToPermissionString());

            var deletedPermissions = (Permissions)~(~(long)roleBefore.Permissions | (long)roleAfter.Permissions);
            builder.AddField("Deleted permissions", deletedPermissions.ToPermissionString());
        }
        if (builder.Fields.Count == 0) return;

        var channel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
        await channel.SendMessageAsync(embed: builder.Build());
    }
}
