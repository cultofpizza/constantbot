﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events.Abstractions;

public interface IEventModule
{
    public void Register(DiscordSocketClient client);
}
