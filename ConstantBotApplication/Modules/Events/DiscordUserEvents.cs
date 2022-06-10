using ConstantBotApplication.Domain;
using ConstantBotApplication.Modules.Events.Abstractions;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events
{
    internal class DiscordUserEvents : IDiscordUserEventHandler
    {
        private readonly BotContext _context;
        private readonly DiscordSocketClient _client;

        public DiscordUserEvents(BotContext context, DiscordSocketClient client)
        {
            _context = context;
            _client = client;
        }

        public async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;
            
            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Red)
                .WithDescription($"{Emoji.Parse(":hammer:")} User {user.Username}({user.Mention}) was banned!")
                .AddField("Reason", (await guild.GetBanAsync(user)).Reason);


            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserCommandExecuted(SocketUserCommand cmd)
        {
            var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == cmd.GuildId).SingleOrDefaultAsync();
            if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new EmbedBuilder()
                .WithAuthor(cmd.User)
                .WithFooter($"ID: {cmd.User.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Magenta)
                .WithDescription($"{Emoji.Parse(":incoming_envelope:")} User {cmd.User.Username}({cmd.User.Mention}) is using command {cmd.CommandName}")
                .AddField("Channel", cmd.Channel.Name);


            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserJoined(SocketGuildUser user)
        {
            var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == user.Guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Green)
                .WithDescription($"{Emoji.Parse(":confetti_ball:")} User {user.Username}({user.Mention}) joined your server!")
                .WithImageUrl(user.GetAvatarUrl())
                ;


            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserLeft(SocketGuild guild, SocketUser user)
        {
            var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Red)
                .WithDescription($"{Emoji.Parse(":confetti_ball:")} User {user.Username}({user.Mention}) left your server!")
                .WithImageUrl(user.GetAvatarUrl())
                ;


            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserUnbanned(SocketUser user, SocketGuild guild)
        {
            var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Green)
                .WithDescription($"{Emoji.Parse(":hammer:")} User {user.Username}({user.Mention}) was unbanned!");

            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserUpdated(SocketUser userBefore, SocketUser userAfter)
        {
            HashSet<ulong> guildIds = new HashSet<ulong>();
            foreach (var item in userBefore.MutualGuilds.Select(i => i.Id).ToArray()) guildIds.Add(item);
            foreach (var item in userAfter.MutualGuilds.Select(i => i.Id).ToArray()) guildIds.Add(item);
            var settings = await _context.GuildSettings.ToListAsync();
            foreach (var item in settings)
            {
                if (!guildIds.Contains(item.GuilId)) continue;
                if (!item.MonitoringEnable || !item.MonitorChannelId.HasValue) guildIds.Remove(item.GuilId);
            }

            var builder = new EmbedBuilder()
                .WithAuthor(userAfter)
                .WithFooter($"ID: {userAfter.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Green)
                .WithDescription($"{Emoji.Parse(":question:")} User {userAfter.Username}({userAfter.Mention}) has updated profile");
            if (userBefore.AvatarId != userAfter.AvatarId)
                builder.AddField("Avatar", $"[Before]({userBefore.GetAvatarUrl()}) to [After]({userAfter.GetAvatarUrl()})");
            if (userBefore.Username != userAfter.Username)
                builder.AddField("Username", $"{userBefore.Username} to {userAfter.Username}");

            foreach (var item in guildIds)
            {
                var channel = (SocketTextChannel) await _client.GetChannelAsync(settings.Where(i=>i.GuilId==item).FirstOrDefault().MonitorChannelId.Value);
                await channel.SendMessageAsync(embed: builder.Build());
            }
        }

        public async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState stateBefore, SocketVoiceState stateAfter)
        {
            if (stateBefore.VoiceChannel == stateAfter.VoiceChannel) return;
            var guildId = stateBefore.VoiceChannel !=null ? stateBefore.VoiceChannel.Guild.Id : stateAfter.VoiceChannel.Guild.Id;
            var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == guildId).SingleOrDefaultAsync();
            if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;
            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp();
            if (stateBefore.VoiceChannel == null)
            {
                builder.WithColor(Color.Green)
                    .WithDescription($"{Emoji.Parse(":inbox_tray:")} User {user.Username}({user.Mention}) joined channel {stateAfter.VoiceChannel.Mention}");

            }
            else if (stateAfter.VoiceChannel == null)
            {
                builder.WithColor(Color.Red)
                    .WithDescription($"{Emoji.Parse(":outbox_tray:")} User {user.Username}({user.Mention}) left channel {stateBefore.VoiceChannel.Mention}");

            }
            else
            {
                builder.WithColor(Color.Orange)
                    .WithDescription($"{user.Username}({user.Mention}) was moved")
                    .AddField("Channel Before",stateBefore.VoiceChannel.Mention, true)
                    .AddField("Channel After",stateAfter.VoiceChannel.Mention, true);

            }
            await channel.SendMessageAsync(embed: builder.Build());
        }
    }
}
