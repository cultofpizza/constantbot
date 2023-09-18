using ConstantBotApplication.Domain;
using ConstantBotApplication.Modules.Events.Abstractions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events;

public class DiscordChannelEvents : IEventModule
{
    private readonly BotContext _context;
    private readonly DiscordClient _client;

    public DiscordChannelEvents(BotContext context, DiscordClient client)
    {
        _context = context;
        _client = client;
    }

    public void Register(DiscordClient client)
    {
        client.ChannelCreated += ChannelCreated;
        client.ChannelDeleted += ChannelDeleted;
        client.ChannelUpdated += ChannelUpdated;
    }

    public async Task ChannelCreated(DiscordClient client, 
        ChannelCreateEventArgs args)
    {
        var channel = args.Channel;
        if (channel == null) return;
        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == channel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ChannelMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new DiscordEmbedBuilder()
                .WithAuthor(channel.Guild.Name, channel.Guild.IconUrl)
                .WithFooter($"ID: {channel.Id}")
                .WithTimestamp(DateTime.UtcNow)
                //.WithCurrentTimestamp()
                .WithColor(DiscordColor.Green)
                .WithDescription($"{DiscordEmoji.FromName(client, ":book:")} Created channel ``{channel.Name}``");

        var monitorChannel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        foreach (var channelPermissions in channel.PermissionOverwrites)
        {
            string targetMention;
            if (channelPermissions.Type == OverwriteType.Role)
            {
                targetMention = channel.Guild.GetRole(channelPermissions.Id).Name;
            }
            else
            {
                targetMention = (await channel.Guild.GetMemberAsync(channelPermissions.Id)).DisplayName;
            }

            builder.AddField($"``{targetMention}`` allow-permissions", channelPermissions.Allowed.ToPermissionString());

            builder.AddField($"``{targetMention}`` deny-permissions", channelPermissions.Denied.ToPermissionString());
        }
        if (builder.Fields.Count > 0)
            await monitorChannel.SendMessageAsync(embed: builder.Build());
    }

    public async Task ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs args)
    {
        var channel = args.Channel;
        if (channel == null) return;
        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == channel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ChannelMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new DiscordEmbedBuilder()
                .WithAuthor(channel.Guild.Name, channel.Guild.IconUrl)
                .WithFooter($"ID: {channel.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Red)
                .WithDescription($"{DiscordEmoji.FromName(client, ":x:")} Deleted channel ``{channel.Name}``");

        var monitorChannel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
        await monitorChannel.SendMessageAsync(embed: builder.Build());
    }

    public async Task ChannelUpdated(DiscordClient client, ChannelUpdateEventArgs args)
    {
        var channelBefore = args.ChannelBefore;
        var channelAfter = args.ChannelAfter;
        if (channelBefore == null || channelAfter == null) return;
        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == channelAfter.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ChannelMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new DiscordEmbedBuilder()
                .WithAuthor(channelAfter.Guild.Name, channelAfter.Guild.IconUrl)
                .WithFooter($"ID: {channelAfter.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.Gold)
                .WithDescription($"{DiscordEmoji.FromName(client, ":pencil2:")} Updated channel ``{channelAfter.Name}``");

        if (channelBefore.Name != channelAfter.Name)
            builder.AddField("Old name", channelBefore.Name);

        var monitorChannel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        if (channelBefore.PermissionOverwrites.Count == channelAfter.PermissionOverwrites.Count)
        {
            foreach (var overwriteBefore in channelBefore.PermissionOverwrites)
            {
                if (overwriteBefore.Allowed != channelAfter.PermissionOverwrites.Where(i => i.Id == overwriteBefore.Id && i.Type == overwriteBefore.Type).First().Allowed ||
                    overwriteBefore.Denied != channelAfter.PermissionOverwrites.Where(i => i.Id == overwriteBefore.Id && i.Type == overwriteBefore.Type).First().Denied)
                {
                    string permissionsOwner = overwriteBefore.Type == OverwriteType.Role ? channelAfter.Guild.GetRole(overwriteBefore.Id).Name : (await channelAfter.Guild.GetMemberAsync(overwriteBefore.Id)).DisplayName;

                    var allowPermissionsBefore = overwriteBefore.Allowed;
                    var allowPermissionsAfter = channelAfter.PermissionOverwrites.Where(i => i.Id == overwriteBefore.Id && i.Type == overwriteBefore.Type).First().Allowed;

                    var addedAllow = (Permissions)~(~(long)allowPermissionsAfter | (long)allowPermissionsBefore);
                    builder.AddField($"Added allow-permissions for ``{permissionsOwner}``", addedAllow.ToPermissionString());

                    var removedAllow = (Permissions)~(~(long)allowPermissionsBefore | (long)allowPermissionsAfter);
                    builder.AddField($"Deleted allow-permissions for ``{permissionsOwner}``", removedAllow.ToPermissionString());

                    var denyPermissionsBefore = overwriteBefore.Denied;
                    var denyPermissionsAfter = channelAfter.PermissionOverwrites.Where(i => i.Id == overwriteBefore.Id && i.Type == overwriteBefore.Type).First().Denied;

                    var addedDeny = (Permissions)~(~(long)denyPermissionsAfter | (long)denyPermissionsBefore);
                    builder.AddField($"Added deny-permissions for ``{permissionsOwner}``", addedDeny.ToPermissionString());

                    var removedDeny = (Permissions)~(~(long)denyPermissionsBefore | (long)denyPermissionsAfter);
                    builder.AddField($"Deleted deny-permissions for ``{permissionsOwner}``", removedDeny.ToPermissionString());

                    //break;
                }
            }
        }
        else if (channelBefore.PermissionOverwrites.Count > channelAfter.PermissionOverwrites.Count)
        {
            var newOverwrite = channelBefore.PermissionOverwrites.Except(channelAfter.PermissionOverwrites).First();
            string permissionsOwner = newOverwrite.Type == OverwriteType.Role ? channelAfter.Guild.GetRole(newOverwrite.Id).Name : (await channelAfter.Guild.GetMemberAsync(newOverwrite.Id)).DisplayName;

            builder.AddField("Added permissions for", $"``{permissionsOwner}``");
        }
        else
        {
            var deletedOverwrite = channelAfter.PermissionOverwrites.Except(channelBefore.PermissionOverwrites).First();
            string permissionsOwner = deletedOverwrite.Type == OverwriteType.Role ? channelAfter.Guild.GetRole(deletedOverwrite.Id).Name : (await channelAfter.Guild.GetMemberAsync(deletedOverwrite.Id)).DisplayName;

            builder.AddField("Deleted permissions for", $"``{permissionsOwner}``");
        }

        await monitorChannel.SendMessageAsync(embed: builder.Build());
    }

}
