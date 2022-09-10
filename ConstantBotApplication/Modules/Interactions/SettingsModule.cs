using ConstantBotApplication.Domain;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
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
    private readonly InteractionService interactions;
    private readonly Emoji enbaledEmoji = Emoji.Parse(":white_check_mark:");
    private readonly Emoji disabledEmoji = Emoji.Parse(":x:"); //":negative_squared_cross_mark:"

    public SettingsModule(BotContext context, InteractionService interactions)
    {
        _context = context;
        this.interactions = interactions;
    }

    [SlashCommand("monitoring", "Configures monitoring")]
    public async Task ShowMonitoringConfig()
    {
        var entry = await _context.Guilds.Where(i => i.GuildId == Context.Guild.Id).FirstOrDefaultAsync();

        var components = GetMonitoringComponents(entry.MonitoringConfig);

        await RespondAsync(components: components);
    }

    [ComponentInteraction("config-monitor-*", true)]
    public async Task ConfigureMonitoring(ushort config, int id)
    {
        var bits = new BitArray(BitConverter.GetBytes(config));
        bits[id] = !bits[id];

        var message = (Context.Interaction as SocketMessageComponent).Message;
        await message.ModifyAsync(i =>
        {
            i.Components = GetMonitoringComponents(bits);
        });
        await RespondAsync();
    }

    [ComponentInteraction("apply-monitor-*", true)]
    public async Task ApplyMonitoring(ushort config)
    {
        var bits = new BitArray(BitConverter.GetBytes(config));

        var entry = await _context.Guilds.Where(i => i.GuildId == Context.Guild.Id).FirstOrDefaultAsync();
        entry.MonitoringConfig = bits;
        await _context.SaveChangesAsync();

        var message = (Context.Interaction as SocketMessageComponent).Message;
        await message.DeleteAsync();
        await RespondAsync();
    }

    [ComponentInteraction("cancel-monitor", true)]
    public async Task ApplyMonitoring()
    {
        var message = (Context.Interaction as SocketMessageComponent).Message;
        await message.DeleteAsync();
        await RespondAsync();
    }


    [SlashCommand("reports","Configures reporting")]
    public async Task SetReportChannel(bool? enabled)
    {
        var entry = await _context.Guilds.Where(i => i.GuildId == Context.Guild.Id).FirstOrDefaultAsync();

        entry.ReportChannelId = enabled.HasValue ? Context.Channel.Id : null;

        await _context.SaveChangesAsync();
        await RespondAsync("Changes applied successfuly!", ephemeral: true);
    }

    private MessageComponent GetMonitoringComponents(BitArray config)
    {
        byte[] bytes = new byte[2];
        config.CopyTo(bytes, 0);
        var configInt = BitConverter.ToUInt16(bytes, 0).ToString();
        var customId = "config-monitor-" + configInt;

        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Configure...")
            .WithCustomId(customId)
            .WithOptions(new List<SelectMenuOptionBuilder>
            {
                new SelectMenuOptionBuilder(){ Emote = config[0]? enbaledEmoji: disabledEmoji, Value = "0", Label = "User monitoring", Description = "Monitors users activity (nicknames, avatars etc)"},
                new SelectMenuOptionBuilder(){ Emote = config[1]? enbaledEmoji: disabledEmoji, Value = "1", Label = "Voice monitoring", Description = "Monitors who and when connects to voice channels"},
                new SelectMenuOptionBuilder(){ Emote = config[2]? enbaledEmoji: disabledEmoji, Value = "2", Label = "Reactions monitoring", Description = "Monitors users` reactions"},
                new SelectMenuOptionBuilder(){ Emote = config[3]? enbaledEmoji: disabledEmoji, Value = "3", Label = "Channel monitoring", Description = "Monitors editing channels"},
                new SelectMenuOptionBuilder(){ Emote = config[4]? enbaledEmoji: disabledEmoji, Value = "4", Label = "Roles monitoring", Description = "Monitors editing roles"},
            });

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder)
            .WithButton("Apply",$"apply-monitor-{configInt}",ButtonStyle.Success)
            .WithButton("Cancel",$"cancel-monitor",ButtonStyle.Secondary)
            .Build();

        return builder;
    }
}
