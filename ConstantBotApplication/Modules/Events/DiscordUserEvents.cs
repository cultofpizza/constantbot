using ConstantBotApplication.Domain;
using ConstantBotApplication.Modules.Events.Abstractions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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
        private readonly DiscordClient _client;

        public DiscordUserEvents(BotContext context, DiscordClient client)
        {
            _context = context;
            _client = client;
        }

        public void Register(DiscordClient client)
        {
            client.UserUpdated += UserUpdated;
            client.GuildMemberAdded += UserJoined;
            client.GuildMemberRemoved += UserLeft;
            client.GuildBanAdded += UserBanned;
            client.GuildBanRemoved += UserUnbanned;
            client.VoiceStateUpdated += UserVoiceStateUpdated;
            client.GuildMemberUpdated += GuildMemberUpdated;
        }

        public async Task GuildMemberUpdated(DiscordClient client, GuildMemberUpdateEventArgs args)
        {
            var userBefore = args.MemberBefore;
            var userAfter = args.MemberAfter;
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == args.Guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new DiscordEmbedBuilder()
                .WithAuthor(userAfter.Username, null, userAfter.AvatarUrl)
                .WithFooter($"ID: {userAfter.Id}")
                .WithTimestamp(DateTime.Now);
            if (!userBefore.Roles.SequenceEqual(userAfter.Roles))
            {
                builder.WithColor(DiscordColor.Blue);
                if (userBefore.Roles.Count() < userAfter.Roles.Count())
                    builder.WithDescription($"{DiscordEmoji.FromName(client, ":gear:")} ``{userAfter.DisplayName}`` was assigned a role ``{userAfter.Roles.Except(userBefore.Roles).FirstOrDefault()}``");
                else
                    builder.WithDescription($"{DiscordEmoji.FromName(client, ":gear:")} ``{userAfter.DisplayName}``\'s role ``{userBefore.Roles.Except(userAfter.Roles).FirstOrDefault()}`` was taken away");
            }
            else
            {
                builder.WithColor(DiscordColor.Gold)
                .WithDescription($"{DiscordEmoji.FromName(client, ":detective:")} ``{userAfter.Nickname ?? userAfter.Username}`` has updated server profile")
                .AddField("User", userAfter.Mention);
                if (userBefore.AvatarUrl != userAfter.AvatarUrl) // Эта херня не работает потому что аватарка до и после почему то всегда одна и таже
                {
                    if (userAfter.AvatarUrl != null)
                        builder.WithImageUrl(userAfter.AvatarUrl);
                    else
                        builder.WithImageUrl(userAfter.DefaultAvatarUrl);
                }
                else if (userBefore.GuildAvatarHash != userAfter.GuildAvatarHash)
                {
                    if (userAfter.GuildAvatarUrl != null)
                        builder.WithImageUrl(userAfter.GuildAvatarUrl);
                    else if (userAfter.AvatarUrl != null)
                        builder.WithImageUrl(userAfter.AvatarUrl);
                    else
                        builder.WithImageUrl(userAfter.DefaultAvatarUrl);
                }
                if (userBefore.Username != userAfter.Username)
                    builder.AddField("Username before", userBefore.Username != null ? $"``{userBefore.Username}``" : "None", true)
                    .AddField("After", userAfter.Username != null ? $"``{userAfter.Username}``" : "None", true);
                else if (userBefore.Nickname != userAfter.Nickname)
                    builder.AddField("Nickname before", userBefore.Nickname != null ? $"``{userBefore.Nickname}``" : "None", true)
                    .AddField("After", userAfter.Nickname != null ? $"``{userAfter.Nickname}``" : "None", true);
            }

            var channel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            if (builder.Fields.Count != 1 || builder.ImageUrl != null)
                await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserBanned(DiscordClient client, GuildBanAddEventArgs args)
        {
            var user = args.Member;
            var guild = args.Guild;
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new DiscordEmbedBuilder()
                .WithAuthor(user.Username, null, user.AvatarUrl)
                .WithFooter($"ID: {user.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Red)
                .WithDescription($"{DiscordEmoji.FromName(client, ":hammer:")} ``{user.Username}`` was banned!")
                .AddField("User", user.Mention, true)
                .AddField("Reason", (await guild.GetBanAsync(user)).Reason, true);


            var channel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserJoined(DiscordClient client, GuildMemberAddEventArgs args)
        {
            var user = args.Member;
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == user.Guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new DiscordEmbedBuilder()
                .WithAuthor(user.Username, null, user.AvatarUrl)
                .WithFooter($"ID: {user.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Green)
                .WithDescription($"{DiscordEmoji.FromName(client, ":confetti_ball:")} ``{user.Username}`` joined your server!")
                .WithImageUrl(user.AvatarUrl)
                .AddField("User", user.Mention);


            var channel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserLeft(DiscordClient client, GuildMemberRemoveEventArgs args)
        {
            var user = args.Member;
            var guild = args.Guild;
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new DiscordEmbedBuilder()
                .WithAuthor(user.Username, null, user.AvatarUrl)
                .WithFooter($"ID: {user.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Red)
                .WithDescription($"{DiscordEmoji.FromName(client, ":door:")} ``{user.Username}`` left your server!")
                .WithImageUrl(user.AvatarUrl)
                .AddField("User", user.Mention);


            var channel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserUnbanned(DiscordClient client, GuildBanRemoveEventArgs args)
        {
            var user = args.Member;
            var guild = args.Guild;
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guild.Id).SingleOrDefaultAsync();
            if (!guildSettings.UserMonitoring || !guildSettings.MonitorChannelId.HasValue) return;

            var builder = new DiscordEmbedBuilder()
                .WithAuthor(user.Username, null, user.AvatarUrl)
                .WithFooter($"ID: {user.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Green)
                .WithDescription($"{DiscordEmoji.FromName(client, ":hammer:")} ``{user.Username}`` was unbanned!")
                .AddField("User", user.Mention);

            var channel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task UserUpdated(DiscordClient client, UserUpdateEventArgs args)
        {
            var userBefore = args.UserBefore;
            var userAfter = args.UserAfter;
            HashSet<ulong> guildIds = new HashSet<ulong>();

            var settings = await _context.Guilds.ToListAsync();
            foreach (var item in settings)
            {
                if (!item.UserMonitoring || !item.MonitorChannelId.HasValue) continue;
                var guild = await client.GetGuildAsync(item.GuildId);
                var user = await guild.GetMemberAsync(userBefore.Id);
                if (user == null) continue;
                guildIds.Add(item.GuildId);
            }

            var builder = new DiscordEmbedBuilder()
                .WithAuthor(userAfter.Username, null, userAfter.AvatarUrl)
                .WithFooter($"ID: {userAfter.Id}")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Gold)
                .WithDescription($"{DiscordEmoji.FromName(client, ":detective:")} ``{userAfter.Username}`` has updated profile")
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
                    var channel = await _client.GetChannelAsync(settings.Where(i => i.GuildId == item).FirstOrDefault().MonitorChannelId.Value);
                    await channel.SendMessageAsync(embed: embed);
                }
            }
        }

        public async Task UserVoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs args)
        {
            var stateBefore = args.Before; 
            var stateAfter = args.After;
            var user = args.User;

            if (stateBefore?.Channel == stateAfter?.Channel) return;
            var guildId = stateBefore != null ? stateBefore.Channel.Guild.Id : stateAfter.Channel.Guild.Id;
            var guildSettings = await _context.Guilds.AsQueryable().Where(i => i.GuildId == guildId).SingleOrDefaultAsync();
            if (!guildSettings.VoiceMonitoring || !guildSettings.MonitorChannelId.HasValue) return;
            var channel = await _client.GetChannelAsync(guildSettings.MonitorChannelId.Value);
            var builder = new DiscordEmbedBuilder()
                .WithAuthor(user.Username, null, user.AvatarUrl)
                .WithFooter($"Session ID: {args.SessionId}")
                .WithTimestamp(DateTime.Now);
            if (stateBefore == null)
            {
                builder.WithColor(DiscordColor.Green)
                    .WithDescription($"{DiscordEmoji.FromName(client, ":inbox_tray:")} ``{user.Username}`` joined channel {stateAfter.Channel.Mention}");

            }
            else if (stateAfter.Channel == null)
            {
                builder.WithColor(DiscordColor.Red)
                    .WithDescription($"{DiscordEmoji.FromName(client, ":outbox_tray:")} ``{user.Username}`` left channel {stateBefore.Channel.Mention}");

            }
            else
            {
                builder.WithColor(DiscordColor.Orange)
                    .WithDescription($"``{user.Username}`` was moved")
                    .AddField("Channel before", stateBefore.Channel.Mention, true)
                    .AddField("After", stateAfter.Channel.Mention, true);

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
