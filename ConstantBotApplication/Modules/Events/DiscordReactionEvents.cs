using ConstantBotApplication.Domain;
using ConstantBotApplication.Modules.Events.Abstractions;
using Discord;
using Discord.WebSocket;
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
    private readonly DiscordSocketClient _client;

    public DiscordReactionEvents(BotContext context, DiscordSocketClient client)
    {
        _context = context;
        _client = client;
    }
    public void Register(DiscordSocketClient client)
    {
        client.ReactionAdded += ReactionAdded;
        client.ReactionRemoved += ReactionRemoved;
        client.ReactionsCleared += ReactionCleared;
        client.ReactionsRemovedForEmote += ReactionClearedForEmote;
    }

    private async Task ReactionClearedForEmote(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, IEmote emote)
    {
        var guildChannel = (await channel.GetOrDownloadAsync()) as SocketGuildChannel;
        var guildMessage = (await message.GetOrDownloadAsync()) as SocketUserMessage;
        if (guildChannel == null || guildMessage == null) return;

        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildChannel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ReactionsMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
        var monitoringChannel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        var builder = new EmbedBuilder()
                .WithAuthor(guildMessage.Author)
                .WithFooter($"ID: {guildMessage.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.DarkRed)
                .WithDescription($"{Emoji.Parse(":eyes:")} Reactions of emote {emote} to message in ``{guildChannel.Name}`` was cleared")
                .AddField("Message Author", guildMessage.Author.Mention, true)
                .AddField("Message Timestamp", guildMessage.Timestamp, true);


        await monitoringChannel.SendMessageAsync(embed: builder.Build());
    }

    private async Task ReactionCleared(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
    {
        var guildChannel = (await channel.GetOrDownloadAsync()) as SocketGuildChannel;
        var guildMessage = (await message.GetOrDownloadAsync()) as SocketUserMessage;
        if (guildChannel == null || guildMessage == null) return;

        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildChannel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ReactionsMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
        var monitoringChannel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        var builder = new EmbedBuilder()
                .WithAuthor(guildMessage.Author)
                .WithFooter($"ID: {guildMessage.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.DarkRed)
                .WithDescription($"{Emoji.Parse(":eyes:")} Reactions to message in ``{guildChannel.Name}`` was cleared")
                .AddField("Message Author", guildMessage.Author.Mention, true)
                .AddField("Message Timestamp", guildMessage.Timestamp, true);


        await monitoringChannel.SendMessageAsync(embed: builder.Build());
    }

    private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        var guildChannel = (await channel.GetOrDownloadAsync()) as SocketGuildChannel;
        var guildMessage = (await message.GetOrDownloadAsync()) as SocketUserMessage;
        if (guildChannel == null || guildMessage == null) return;

        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildChannel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ReactionsMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
        var monitoringChannel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        var builder = new EmbedBuilder()
                .WithAuthor(reaction.User.Value)
                .WithFooter($"ID: {guildMessage.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Red)
                .WithDescription($"{Emoji.Parse(":eyes:")} ``{reaction.User.Value.Username}`` reaction {reaction.Emote} to message in ``{guildChannel.Name}`` was removed")
                .AddField("Message Author", guildMessage.Author.Mention, true)
                .AddField("Message Timestamp", guildMessage.Timestamp, true);


        await monitoringChannel.SendMessageAsync(embed: builder.Build());
    }

    private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        var guildChannel = (await channel.GetOrDownloadAsync()) as SocketGuildChannel;
        var guildMessage = (await message.GetOrDownloadAsync()) as SocketUserMessage;
        if(guildChannel == null || guildMessage == null) return;
        
        var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildChannel.Guild.Id).SingleOrDefaultAsync();
        if (!guildSettings.ReactionsMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
        var monitoringChannel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);

        var builder = new EmbedBuilder()
                .WithAuthor(reaction.User.Value)
                .WithFooter($"ID: {guildMessage.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Green)
                .WithDescription($"{Emoji.Parse(":eyes:")} ``{reaction.User.Value.Username}`` reacted to message in ``{guildChannel.Name}`` with {reaction.Emote}")
                .AddField("Message Author", guildMessage.Author.Mention,true)
                .AddField("Message Timestamp", guildMessage.Timestamp, true);


        await monitoringChannel.SendMessageAsync(embed: builder.Build());
    }
}
