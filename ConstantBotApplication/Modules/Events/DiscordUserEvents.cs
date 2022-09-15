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
    public class DiscordUserEvents : IEventModule
    {
        private readonly BotContext _context;
        private readonly DiscordSocketClient _client;

        public DiscordUserEvents(BotContext context, DiscordSocketClient client)
        {
            _context = context;
            _client = client;
        }

        public void Register(DiscordSocketClient client)
        {
            client.UserUpdated += UserUpdated;
            client.UserJoined += UserJoined;
            client.UserLeft += UserLeft;
            client.UserBanned += UserBanned;
            client.UserUnbanned += UserUnbanned;
            client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            client.GuildMemberUpdated += GuildMemberUpdated;
        }

        public async Task GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> userBefore, SocketGuildUser userAfter)
        {
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == userAfter.Guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new EmbedBuilder()
                .WithAuthor(userAfter)
                .WithFooter($"ID: {userAfter.Id}")
                .WithCurrentTimestamp();
            if (userBefore.HasValue && !userBefore.Value.Roles.SequenceEqual(userAfter.Roles))
            {
                builder.WithColor(Color.Blue);
                if (userBefore.Value.Roles.Count() < userAfter.Roles.Count())
                    builder.WithDescription($"{Emoji.Parse(":gear:")} ``{userAfter.DisplayName}`` was assigned a role ``{userAfter.Roles.Except(userBefore.Value.Roles).FirstOrDefault()}``");
                else
                    builder.WithDescription($"{Emoji.Parse(":gear:")} ``{userAfter.DisplayName}``\'s role ``{userBefore.Value.Roles.Except(userAfter.Roles).FirstOrDefault()}`` was taken away");
            }
            else
            {
                builder.WithColor(Color.Gold)
                .WithDescription($"{Emoji.Parse(":detective:")} ``{userAfter.Nickname ?? userAfter.Username}`` has updated server profile")
                .AddField("User", userAfter.Mention);
                if (userBefore.HasValue && userBefore.Value.DisplayAvatarId != userAfter.DisplayAvatarId)
                {
                    //builder.AddField("Server avatar before", userBefore.Value.AvatarId != null ? $"[Avatar]({userBefore.Value.GetGuildAvatarUrl()})" : $"[Avatar]({userBefore.Value.GetDefaultAvatarUrl()})");
                    //if (userAfter.AvatarId != null)
                    //    builder.WithImageUrl(userAfter.GetGuildAvatarUrl());
                    //else
                    //    builder.WithImageUrl(userAfter.GetDefaultAvatarUrl()); Old avatars(except default) are not accessible
                    if (userAfter.DisplayAvatarId != null)
                        builder.WithImageUrl(userAfter.GetDisplayAvatarUrl(size: 4096));
                    else
                        builder.WithImageUrl(userAfter.GetDefaultAvatarUrl());
                }
                if (userBefore.HasValue && userBefore.Value.Nickname != userAfter.Nickname)
                    builder.AddField("Nickname before", userBefore.Value.Nickname != null ? $"``{userBefore.Value.Nickname}``" : "None", true)
                    .AddField("After", userAfter.Nickname != null ? $"``{userAfter.Nickname}``" : "None", true);
            }

            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            if (builder.Fields.Count != 1 || builder.ImageUrl != null)
                await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Red)
                .WithDescription($"{Emoji.Parse(":hammer:")} ``{user.Username}`` was banned!")
                .AddField("User", user.Mention, true)
                .AddField("Reason", (await guild.GetBanAsync(user)).Reason, true);


            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserJoined(SocketGuildUser user)
        {
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == user.Guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Green)
                .WithDescription($"{Emoji.Parse(":confetti_ball:")} ``{user.Username}`` joined your server!")
                .WithImageUrl(user.GetAvatarUrl())
                .AddField("User", user.Mention);


            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserLeft(SocketGuild guild, SocketUser user)
        {
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Red)
                .WithDescription($"{Emoji.Parse(":confetti_ball:")} ``{user.Username}`` left your server!")
                .WithImageUrl(user.GetAvatarUrl())
                .AddField("User", user.Mention);


            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserUnbanned(SocketUser user, SocketGuild guild)
        {
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Green)
                .WithDescription($"{Emoji.Parse(":hammer:")} ``{user.Username}`` was unbanned!")
                .AddField("User", user.Mention);

            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserUpdated(SocketUser userBefore, SocketUser userAfter)
        {
            HashSet<ulong> guildIds = new HashSet<ulong>();
            foreach (var item in userBefore.MutualGuilds.Select(i => i.Id).ToArray()) guildIds.Add(item);
            foreach (var item in userAfter.MutualGuilds.Select(i => i.Id).ToArray()) guildIds.Add(item);
            var settings = await _context.Guilds.ToListAsync();
            foreach (var item in settings)
            {
                if (!guildIds.Contains(item.GuildId)) continue;
                if (!item.UserMonitoring || !item.MonitorChannelId.HasValue) guildIds.Remove(item.GuildId);
            }

            var builder = new EmbedBuilder()
                .WithAuthor(userAfter)
                .WithFooter($"ID: {userAfter.Id}")
                .WithCurrentTimestamp()
                .WithColor(Color.Gold)
                .WithDescription($"{Emoji.Parse(":detective:")} ``{userAfter.Username}`` has updated profile")
                .AddField("User", userAfter.Mention);
            //if (userBefore.AvatarId != userAfter.AvatarId)
            //{
            //    //builder.AddField("Avatar before", userBefore.AvatarId != null ? $"[Avatar]({userBefore.GetAvatarUrl(size: 512)})" : $"[Avatar]({userBefore.GetDefaultAvatarUrl()})");
            //    //if (userAfter.AvatarId != null)
            //    //    builder.WithImageUrl(userAfter.GetAvatarUrl());
            //    //else
            //    //    builder.WithImageUrl(userAfter.GetDefaultAvatarUrl()); Old avatars(except default) are not accessible
            //    if (userAfter.AvatarId != null)
            //        builder.WithImageUrl(userAfter.GetAvatarUrl(size: 4096));
            //    else
            //        builder.WithImageUrl(userAfter.GetDefaultAvatarUrl());
            //}
            if (userBefore.Username != userAfter.Username)
                builder.AddField("Username before", $"``{userBefore.Username}``", true)
                .AddField("After", $"``{userAfter.Username}``", true);// Both fields must be in-line=true to be inline?!

            if (builder.Fields.Count != 1 || builder.ImageUrl != null)
            {
                var embed = builder.Build();
                foreach (var item in guildIds)
                {
                    var channel = (SocketTextChannel)await _client.GetChannelAsync(settings.Where(i => i.GuildId == item).FirstOrDefault().MonitorChannelId.Value);
                    await channel.SendMessageAsync(embed: embed);
                }
            }
        }

        public async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState stateBefore, SocketVoiceState stateAfter)
        {
            if (stateBefore.VoiceChannel == stateAfter.VoiceChannel) return;
            var guildId = stateBefore.VoiceChannel != null ? stateBefore.VoiceChannel.Guild.Id : stateAfter.VoiceChannel.Guild.Id;
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildId).SingleOrDefaultAsync();
            if (!guildSettings.VoiceMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
            var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithFooter($"ID: {user.Id}")
                .WithCurrentTimestamp();
            if (stateBefore.VoiceChannel == null)
            {
                builder.WithColor(Color.Green)
                    .WithDescription($"{Emoji.Parse(":inbox_tray:")} ``{user.Username}`` joined channel {stateAfter.VoiceChannel.Mention}");

            }
            else if (stateAfter.VoiceChannel == null)
            {
                builder.WithColor(Color.Red)
                    .WithDescription($"{Emoji.Parse(":outbox_tray:")} ``{user.Username}`` left channel {stateBefore.VoiceChannel.Mention}");

            }
            else
            {
                builder.WithColor(Color.Orange)
                    .WithDescription($"``{user.Username}`` was moved")
                    .AddField("Channel before", stateBefore.VoiceChannel.Mention, true)
                    .AddField("After", stateAfter.VoiceChannel.Mention, true);

            }
            await channel.SendMessageAsync(embed: builder.Build());
        }

        //public async Task UserCommandExecuted(SocketUserCommand cmd)
        //{
        //    var guildSettings = await _context.GuildSettings.AsQueryable().Where(i => i.GuilId == cmd.GuildId).SingleOrDefaultAsync();
        //    if (!guildSettings.MonitoringEnable || !guildSettings.MonitorChannelId.HasValue) return;

        //    var builder = new EmbedBuilder()
        //        .WithAuthor(cmd.User)
        //        .WithFooter($"ID: {cmd.User.Id}")
        //        .WithCurrentTimestamp()
        //        .WithColor(Color.Magenta)
        //        .WithDescription($"{Emoji.Parse(":incoming_envelope:")} ``{cmd.User.Username}``({cmd.User.Mention}) is using command {cmd.CommandName}")
        //        .AddField("User", cmd.User.Mention, true)
        //        .AddField("Channel", cmd.Channel.Name, true);


        //    var channel = (SocketTextChannel)await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
        //    await channel.SendMessageAsync(embed: builder.Build());
        //}
    }
}
