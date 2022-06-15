using Discord.WebSocket;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events.Abstractions;

public interface IDiscordRoleEventHandler
{
    Task RoleCreated(SocketRole role);
    Task RoleDeleted(SocketRole role);
    Task RoleUpdated(SocketRole roleBefore, SocketRole roleAfter);
}
