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
    private readonly IDiscordRoleEventHandler _roleEvents;
    private readonly IDiscordChannelEventHandler _channelEvents;

    public EventHandler(DiscordSocketClient client, IDiscordUserEventHandler userEvents, IDiscordRoleEventHandler roleEvents, IDiscordChannelEventHandler channelEvents)
    {
        _client = client;
        _userEvents = userEvents;
        _roleEvents = roleEvents;
        _channelEvents = channelEvents;
    }

    public void RegisterEvents()
    {
        RegisterUserEvents();

        RegisterRoleEvents();

        RegisterChannelEvents();

        RegisterTestEvents();
    }

    public void RegisterUserEvents()
    {
        _client.UserUpdated += _userEvents.UserUpdated;
        _client.UserJoined += _userEvents.UserJoined;
        _client.UserLeft += _userEvents.UserLeft;
        _client.UserBanned += _userEvents.UserBanned;
        _client.UserUnbanned += _userEvents.UserUnbanned;
        _client.UserVoiceStateUpdated += _userEvents.UserVoiceStateUpdated;
        _client.GuildMemberUpdated += _userEvents.GuildMemberUpdated;
    }

    public void RegisterRoleEvents()
    {
        _client.RoleCreated += _roleEvents.RoleCreated;
        _client.RoleDeleted += _roleEvents.RoleDeleted;
        _client.RoleUpdated += _roleEvents.RoleUpdated;
    }

    public void RegisterChannelEvents()
    {
        _client.ChannelCreated += _channelEvents.ChannelCreated;
        _client.ChannelDestroyed += _channelEvents.ChannelDeleted;
        _client.ChannelUpdated +=_channelEvents.ChannelUpdated;
    }

    public void RegisterTestEvents()
    {
        //_client.ChannelCreated ChannelEvents

        //_client.GuildScheduledEventCreated Events Events

        //_client.ReactionAdded Reactions to bot messages

        //_client.UserJoined Welcome to new users
    }
}
