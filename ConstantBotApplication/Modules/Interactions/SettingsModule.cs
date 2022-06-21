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

[Group("setup","Configures...")]
[RequireUserPermission(Discord.GuildPermission.Administrator)]
[RequireBotPermission(Discord.ChannelPermission.SendMessages)]
public class SettingsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BotContext _context;

    public SettingsModule(BotContext context)
    {
        _context = context;
    }

    [SlashCommand("monitoring", "Configures monitoring")]
    public async Task SetMonitoringChannel(bool? enabled)
    {
        var entry = await _context.GuildSettings.Where(i => i.GuilId == Context.Guild.Id).FirstOrDefaultAsync();

        entry.MonitoringEnable = enabled.HasValue ? enabled.Value : true;
        entry.MonitorChannelId = Context.Channel.Id;

        await _context.SaveChangesAsync();
        await RespondAsync("Changes applied successfuly!", ephemeral: true);
    }

    [SlashCommand("reports","Configures reporting")]
    public async Task SetReportChannel(bool? enabled)
    {
        var entry = await _context.GuildSettings.Where(i => i.GuilId == Context.Guild.Id).FirstOrDefaultAsync();

        entry.ReportChannelId = enabled.HasValue ? Context.Channel.Id : null;

        await _context.SaveChangesAsync();
        await RespondAsync("Changes applied successfuly!", ephemeral: true);
    }
}
