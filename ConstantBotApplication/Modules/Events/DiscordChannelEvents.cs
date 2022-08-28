using ConstantBotApplication.Domain;
using ConstantBotApplication.Modules.Events.Abstractions;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events;

public class DiscordChannelEvents : IEventModule
{
    private readonly BotContext _context;
    private readonly DiscordSocketClient _client;

    public DiscordChannelEvents(BotContext context, DiscordSocketClient client)
    {
        _context = context;
        _client = client;
    }

    public void Register(DiscordSocketClient client)
    {
        client.ChannelCreated += ChannelCreated;
        client.ChannelDestroyed += ChannelDeleted;
        client.ChannelUpdated += ChannelUpdated;
    }

    public async Task ChannelCreated(SocketChannel ch)
    {
        var channel = ch as SocketGuildChannel;
        if (channel == null) return;
        var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == channel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new EmbedBuilder()
                .WithAuthor(channel.Guild.Name, channel.Guild.IconUrl)
                .WithFooter($"ID: {channel.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Green)
                .WithDescription($"{Emoji.Parse(":book:")} Created channel ``{channel.Name}``");

        var monitorChannel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        foreach (var channelPermissions in channel.PermissionOverwrites)
        {
            string targetMention;
            if (channelPermissions.TargetType == PermissionTarget.Role)
            {
                targetMention = channel.Guild.GetRole(channelPermissions.TargetId).Name;
            }
            else
            {
                targetMention = channel.Guild.GetUser(channelPermissions.TargetId).DisplayName;
            }
            var allowPermissions = channelPermissions.Permissions.ToAllowList();

            if (allowPermissions.Count > 0)
            {
                string permissionsString = allowPermissions[0].ToString();
                for (int i = 1; i < allowPermissions.Count; i++)
                {
                    permissionsString += ", " + allowPermissions[i].ToString();
                }
                builder.AddField($"``{targetMention}`` allow-permissions", permissionsString);
            }
            if (builder.Fields.Count == 25)
            {
                await monitorChannel.SendMessageAsync(embed: builder.Build());
                builder.Fields.Clear();
            }

            var denyPermissions = channelPermissions.Permissions.ToDenyList();

            if (denyPermissions.Count > 0)
            {
                string permissionsString = denyPermissions[0].ToString();
                for (int i = 1; i < denyPermissions.Count; i++)
                {
                    permissionsString += ", " + denyPermissions[i].ToString();
                }
                builder.AddField($"``{targetMention}`` deny-permissions", permissionsString);
            }
            if (builder.Fields.Count == 25)
            {
                await monitorChannel.SendMessageAsync(embed: builder.Build());
                builder.Fields.Clear();
            }
        }
        if (builder.Fields.Count > 0)
            await monitorChannel.SendMessageAsync(embed: builder.Build());
    }

    public async Task ChannelDeleted(SocketChannel ch)
    {
        var channel = ch as SocketGuildChannel;
        if (channel == null) return;
        var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == channel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new EmbedBuilder()
                .WithAuthor(channel.Guild.Name, channel.Guild.IconUrl)
                .WithFooter($"ID: {channel.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Red)
                .WithDescription($"{Emoji.Parse(":x:")} Deleted channel ``{channel.Name}``");

        var monitorChannel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
        await monitorChannel.SendMessageAsync(embed: builder.Build());
    }

    public async Task ChannelUpdated(SocketChannel chBefore, SocketChannel chAfter)
    {
        var channelBefore = chBefore as SocketGuildChannel;
        var channelAfter = chAfter as SocketGuildChannel;
        if (channelBefore == null || channelAfter == null) return;
        var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == channelAfter.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

        var builder = new EmbedBuilder()
                .WithAuthor(channelAfter.Guild.Name, channelAfter.Guild.IconUrl)
                .WithFooter($"ID: {channelAfter.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Gold)
                .WithDescription($"{Emoji.Parse(":pencil2:")} Updated channel ``{channelAfter.Name}``");

        if (channelBefore.Name != channelAfter.Name)
            builder.AddField("Old name", channelBefore.Name);

        var monitorChannel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        if (channelBefore.PermissionOverwrites.Count == channelAfter.PermissionOverwrites.Count)
        {
            foreach (var overwriteBefore in channelBefore.PermissionOverwrites)
            {
                if (overwriteBefore.Permissions.AllowValue != channelAfter.PermissionOverwrites.Where(i => i.TargetId == overwriteBefore.TargetId && i.TargetType == overwriteBefore.TargetType).First().Permissions.AllowValue ||
                    overwriteBefore.Permissions.DenyValue != channelAfter.PermissionOverwrites.Where(i => i.TargetId == overwriteBefore.TargetId && i.TargetType == overwriteBefore.TargetType).First().Permissions.DenyValue)
                {
                    string permissionsOwner = overwriteBefore.TargetType == PermissionTarget.Role ? channelAfter.Guild.GetRole(overwriteBefore.TargetId).Name : channelAfter.Guild.GetUser(overwriteBefore.TargetId).DisplayName;

                    var allowPermissionsBefore = overwriteBefore.Permissions.ToAllowList();
                    var allowPermissionsAfter = channelAfter.PermissionOverwrites.Where(i => i.TargetId == overwriteBefore.TargetId && i.TargetType == overwriteBefore.TargetType).First().Permissions.ToAllowList();


                    var newAllowPermissions = new List<string>();
                    foreach (var item in allowPermissionsAfter)
                    {
                        if (!allowPermissionsBefore.Contains(item))
                            newAllowPermissions.Add(item.ToString());
                    }

                    if (newAllowPermissions.Count > 0)
                    {
                        string permissionsString = newAllowPermissions[0];
                        for (int i = 1; i < newAllowPermissions.Count; i++)
                        {
                            permissionsString += ", " + newAllowPermissions[i].ToString();
                        }
                        builder.AddField($"Added allow-permissions for ``{permissionsOwner}``", permissionsString);
                    }

                    var deletedAllowPermissions = new List<string>();
                    foreach (var item in allowPermissionsBefore)
                    {
                        if (!allowPermissionsAfter.Contains(item))
                            deletedAllowPermissions.Add(item.ToString());
                    }

                    if (deletedAllowPermissions.Count > 0)
                    {
                        string permissionsString = deletedAllowPermissions[0];
                        for (int i = 1; i < deletedAllowPermissions.Count; i++)
                        {
                            permissionsString += ", " + deletedAllowPermissions[i].ToString();
                        }
                        builder.AddField($"Deleted allow-permissions for ``{permissionsOwner}``", permissionsString);
                    }

                    var denyPermissionsBefore = overwriteBefore.Permissions.ToDenyList();
                    var denyPermissionsAfter = channelAfter.PermissionOverwrites.Where(i => i.TargetId == overwriteBefore.TargetId && i.TargetType == overwriteBefore.TargetType).First().Permissions.ToDenyList();

                    var newDenyPermissions = new List<string>();
                    foreach (var item in denyPermissionsAfter)
                    {
                        if (!denyPermissionsBefore.Contains(item))
                            newDenyPermissions.Add(item.ToString());
                    }

                    if (newDenyPermissions.Count > 0)
                    {
                        string permissionsString = newDenyPermissions[0];
                        for (int i = 1; i < newDenyPermissions.Count; i++)
                        {
                            permissionsString += ", " + newDenyPermissions[i].ToString();
                        }
                        builder.AddField($"Added deny-permissions for ``{permissionsOwner}``", permissionsString);
                    }

                    var deletedDenyPermissions = new List<string>();
                    foreach (var item in denyPermissionsBefore)
                    {
                        if (!denyPermissionsAfter.Contains(item))
                            deletedDenyPermissions.Add(item.ToString());
                    }

                    if (deletedDenyPermissions.Count > 0)
                    {
                        string permissionsString = deletedDenyPermissions[0];
                        for (int i = 1; i < deletedDenyPermissions.Count; i++)
                        {
                            permissionsString += ", " + deletedDenyPermissions[i].ToString();
                        }
                        builder.AddField($"Deleted deny-permissions for ``{permissionsOwner}``", permissionsString);
                    }

                    break;
                }
            }
        }
        else if (channelBefore.PermissionOverwrites.Count > channelAfter.PermissionOverwrites.Count)
        {
            var newOverwrite = channelBefore.PermissionOverwrites.Except(channelAfter.PermissionOverwrites).First();
            string permissionsOwner = newOverwrite.TargetType == PermissionTarget.Role ? channelAfter.Guild.GetRole(newOverwrite.TargetId).Name : channelAfter.Guild.GetUser(newOverwrite.TargetId).DisplayName;

            builder.AddField("Added permissions for",$"``{permissionsOwner}``");
        }
        else
        {
            var deletedOverwrite = channelAfter.PermissionOverwrites.Except(channelBefore.PermissionOverwrites).First();
            string permissionsOwner = deletedOverwrite.TargetType == PermissionTarget.Role ? channelAfter.Guild.GetRole(deletedOverwrite.TargetId).Name : channelAfter.Guild.GetUser(deletedOverwrite.TargetId).DisplayName;

            builder.AddField("Deleted permissions for", $"``{permissionsOwner}``");
        }

        await monitorChannel.SendMessageAsync(embed: builder.Build());
    }
}
