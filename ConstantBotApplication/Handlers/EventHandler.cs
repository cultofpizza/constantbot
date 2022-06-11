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

namespace ConstantBotApplication.Handlers;

internal class EventHandler
{
    private readonly DiscordSocketClient _client;
    private readonly IDiscordUserEventHandler _userEvents;
    private readonly BotContext _context;

    public EventHandler(DiscordSocketClient client, IDiscordUserEventHandler userEvents, BotContext context)
    {
        _client = client;
        _userEvents = userEvents;
        _context = context;
    }

    public void RegisterEvents()
    {
        RegisterUserEvents();

        RegisterTestEvents();
    }

    public void RegisterUserEvents()
    {
        _client.UserUpdated += _userEvents.UserUpdated;
        _client.UserJoined += _userEvents.UserJoined;
        _client.UserLeft += _userEvents.UserLeft;
        _client.UserBanned += _userEvents.UserBanned;
        _client.UserCommandExecuted += _userEvents.UserCommandExecuted;
        _client.UserUnbanned += _userEvents.UserUnbanned;
        _client.UserVoiceStateUpdated += _userEvents.UserVoiceStateUpdated;
        _client.GuildMemberUpdated += _userEvents.GuildMemberUpdated;
    }

    public void RegisterTestEvents()
    {

    }
}
