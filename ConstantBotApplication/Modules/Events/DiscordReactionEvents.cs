using ConstantBotApplication.Domain;
using ConstantBotApplication.Modules.Events.Abstractions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events;

public class DiscordReactionEvents : IEventModule
{
    private readonly BotContext _context;
    private readonly DiscordClient _client;

    public DiscordReactionEvents(BotContext context, DiscordClient client)
    {
        _context = context;
        _client = client;
    }
    public void Register(DiscordClient client)
    {
        client.MessageReactionAdded += ReactionAdded;
        client.MessageReactionRemoved += ReactionRemoved;
        client.MessageReactionsCleared += ReactionCleared;
        client.MessageReactionRemovedEmoji += ReactionClearedForEmote;
    }

    private async Task ReactionClearedForEmote(DiscordClient client, MessageReactionRemoveEmojiEventArgs args)
    {
        var guildChannel = args.Channel;
        var guildMessage = args.Message;
        var emote = args.Emoji;
        if (guildChannel == null || guildMessage == null) return;

        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildChannel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ReactionsMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
        var monitoringChannel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        var builder = new DiscordEmbedBuilder()
                .WithFooter($"ID: {guildMessage.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(DiscordColor.DarkRed)
                .WithDescription($"{DiscordEmoji.FromName(client ,":eyes:")} Reactions of emote {emote} to message in ``{guildChannel.Name}`` was cleared")
                .AddField("Message Link", guildMessage.JumpLink.ToString(), true)
                .AddField("Message Timestamp", guildMessage.Timestamp.ToString("u"), true);


        await monitoringChannel.SendMessageAsync(embed: builder.Build());
    }

    private async Task ReactionCleared(DiscordClient client, MessageReactionsClearEventArgs args)
    {
        var guildChannel = args.Channel;
        var guildMessage = args.Message;
        if (guildChannel == null || guildMessage == null) return;

        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildChannel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ReactionsMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
        var monitoringChannel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        var builder = new DiscordEmbedBuilder()
                .WithFooter($"ID: {guildMessage.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.DarkRed)
                .WithDescription($"{DiscordEmoji.FromName(client, ":eyes:")} Reactions to message in ``{guildChannel.Name}`` was cleared")
                .AddField("Message Link", guildMessage.JumpLink.ToString(), true)
                .AddField("Message Timestamp", guildMessage.Timestamp.ToString("u"), true);


        await monitoringChannel.SendMessageAsync(embed: builder.Build());
    }

    private async Task ReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs args)
    {
        var guildChannel = args.Channel;
        var guildMessage = args.Message;
        var emoji = args.Emoji;
        var user = args.User;
        if (guildChannel == null || guildMessage == null) return;

        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildChannel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ReactionsMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
        var monitoringChannel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        var builder = new DiscordEmbedBuilder()
                .WithAuthor(user.Username, null, user.AvatarUrl)
                .WithFooter($"ID: {guildMessage.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Red)
                .WithDescription($"{DiscordEmoji.FromName(client, ":eyes:")} ``{user.Username}`` reaction {emoji} to message in ``{guildChannel.Name}`` was removed")
                .AddField("Message Author", user.Mention, true)
                .AddField("Message Timestamp", guildMessage.Timestamp.ToString("u"), true)
                .AddField("Message Link", guildMessage.JumpLink.ToString());


        await monitoringChannel.SendMessageAsync(embed: builder.Build());
    }

    private async Task ReactionAdded(DiscordClient client, MessageReactionAddEventArgs args)
    {
        var guildChannel = args.Channel;
        var guildMessage = args.Message;
        var emoji = args.Emoji;
        var user = args.User;
        if (guildChannel == null || guildMessage == null) return;

        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildChannel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ReactionsMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
        var monitoringChannel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        var builder = new DiscordEmbedBuilder()
                .WithAuthor(user.Username, null, user.AvatarUrl)
                .WithFooter($"ID: {guildMessage.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Green)
                .WithDescription($"{DiscordEmoji.FromName(client ,":eyes:")} ``{user.Username}`` reacted to message in ``{guildChannel.Name}`` with {emoji}")
                .AddField("Message Author", user.Mention, true)
                .AddField("Message Timestamp", guildMessage.Timestamp.ToString("u"), true)
                .AddField("Message Link", guildMessage.JumpLink.ToString());


        await monitoringChannel.SendMessageAsync(embed: builder.Build());
    }
}
