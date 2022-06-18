using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events.Abstractions;

public interface IDiscordChannelEventHandler
{
    Task ChannelCreated(SocketChannel channel);
    Task ChannelDeleted(SocketChannel channel);
    Task ChannelUpdated(SocketChannel channelBefore, SocketChannel channelAfter);
}
