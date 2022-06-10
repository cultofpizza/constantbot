using ConstantBotApplication.Domain;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Interactions;

public class SettingsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BotContext _context;

    public SettingsModule(BotContext context)
    {
        _context = context;
    }

    [RequireOwner]
    [SlashCommand("monitor", "Configures monitoring")]
    public async Task SetChannel(bool? enabled)
    {
        var entry = await _context.GuildSettings.Where(i => i.GuilId == Context.Guild.Id).FirstOrDefaultAsync();

        entry.MonitoringEnable = enabled.HasValue ? enabled.Value : true;
        entry.MonitorChannelId = Context.Channel.Id;

        await _context.SaveChangesAsync();
        await RespondAsync("Changes applied successfuly!", ephemeral: true);
    }
}
