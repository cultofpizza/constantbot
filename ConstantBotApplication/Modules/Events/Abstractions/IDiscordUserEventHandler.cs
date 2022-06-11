using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events.Abstractions;

internal interface IDiscordUserEventHandler
{
    Task UserUpdated(SocketUser userBefore, SocketUser userAfter);
    Task UserJoined(SocketGuildUser user);
    Task UserLeft(SocketGuild guild, SocketUser user);
    Task UserBanned(SocketUser user, SocketGuild guild);
    Task UserCommandExecuted(SocketUserCommand cmd);
    Task UserUnbanned(SocketUser user, SocketGuild guild);
    Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState stateBefore, SocketVoiceState stateAfter);
    Task GuildMemberUpdated(Cacheable<SocketGuildUser,ulong> userBefore, SocketGuildUser userAfter);
}
