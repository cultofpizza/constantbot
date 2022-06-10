using ConstantBotApplication.Domain;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Events;

internal class MonitoringEventPublisher
{
    private readonly BotContext _context;

    public MonitoringEventPublisher(BotContext context)
    {
        _context = context;
    }

    public void SendMessage(string message)
    {

    }
}
