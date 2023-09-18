using ConstantBotApplication.Domain;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
//using ConstantBotApplication.Modals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Interactions;

[SlashCommandGroup("setup","Configures...")]
[SlashRequireUserPermissions(Permissions.Administrator)]
[SlashRequireBotPermissions(Permissions.SendMessages)]
public class SettingsModule : ApplicationCommandModule
{
    private readonly BotContext _context;
    private readonly DiscordEmoji enbaledEmoji;
    private readonly DiscordEmoji disabledEmoji;

    public SettingsModule(BotContext context, DiscordClient client)
    {
        _context = context;
        enbaledEmoji = DiscordEmoji.FromName(client, ":white_check_mark:");
        disabledEmoji = DiscordEmoji.FromName(client, ":x:"); //":negative_squared_cross_mark:"

    }

    [SlashCommand("monitoring", "Configures monitoring")]
    public async Task ShowMonitoringConfig(InteractionContext context)
    {
        var entry = await _context.Guilds.Where(i => i.GuildId == context.Guild.Id).FirstOrDefaultAsync();

        var components = GetMonitoringComponents(entry.MonitoringConfig);

        await context.CreateResponseAsync(components: components);
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
        entry.MonitorChannelId = Context.Channel.Id;
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

    [SlashCommand("social-attachments", "Configures social attachments")]
    public async Task SetAttachments(SocialActionType actionType)
    {
        var attachments = await _context.SocialAttachments.Where(i => i.Action == actionType && i.GuildId == Context.Guild.Id).ToListAsync();

        var urls = string.Empty;
        foreach (var item in attachments)
        {
            urls += item.Url+ "\n";
        }

        var builder = new ModalBuilder()
            .WithTitle("Attachments")
            .WithCustomId($"social-attachments-{actionType.ToString().ToLower()}")
            .AddTextInput("Enter attachment image urls","urls",TextInputStyle.Paragraph, "Enter urls each in new line", value: urls);

        await RespondWithModalAsync(builder.Build());
    }

    [ModalInteraction("social-attachments-*",true)]
    public async Task SetAttachmentUrls(string actionType, AttachmentModal modal)
    {
        actionType = string.Concat(actionType[0].ToString().ToUpper(), actionType.AsSpan(1));
        var action = Enum.Parse<SocialActionType>(actionType);
        var urls = modal.Urls.Split('\n');
        await DeferAsync();

        var attachments = await _context.SocialAttachments.Where(i => i.Action == action && i.GuildId == Context.Guild.Id).ToListAsync();

        foreach (var item in attachments)
        {
            if (!urls.Contains(item.Url))
            {
                _context.Remove(item);
            }
        }

        foreach (var item in urls)
        {
            if (attachments.Where(i=>i.Url==item).Count()==0)
            {
                _context.Add(new SocialAttachments { Action = action, GuildId = Context.Guild.Id, Url = item });
            }
        }

        await _context.SaveChangesAsync();
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
