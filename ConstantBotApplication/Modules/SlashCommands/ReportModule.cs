using ConstantBotApplication.Domain;
using ConstantBotApplication.Modals;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Interactions
{
    //[EnabledInDm(false)]
    [SlashRequireGuild]
    public class ReportModule : ApplicationCommandModule
    {
        private readonly BotContext context;

        public ReportModule(BotContext context)
        {
            this.context = context;
        }

        [SlashCommand("report","Creates report for moderators")]
        public async Task Report(InteractionContext context)
        {
            if(await context.Guilds.Where(i => i.GuildId == Context.Guild.Id && i.ReportChannelId != null).CountAsync() == 0)
            {
                await RespondAsync("Reporting not enabled on this server", ephemeral: true);
                return;
            }
            await context.CreateResponseAsync(DSharpPlus.InteractionResponseType.Modal,  <ReportModal>("channel_report");
        }
        [modal]
        [ModalInteraction("channel_report")]
        public async Task SaveReport(ReportModal modal)
        {
            var channel = Context.Guild.GetTextChannel((await context.Guilds.Where(i => i.GuildId == Context.Guild.Id && i.ReportChannelId != null).FirstAsync()).ReportChannelId.Value);

            var builder = new EmbedBuilder();
            builder.WithAuthor(Context.User)
                .WithCurrentTimestamp()
                .WithColor(Color.Blue)
                .WithFooter($"ID: {Context.User.Id}")
                .AddField("Channel", Context.Channel.Name)
                .WithTitle("Report")
                .WithDescription(modal.Content);

            await channel.SendMessageAsync(embed: builder.Build());

            await RespondAsync("Your report submitted successfuly", ephemeral: true);
        }

    }
}
