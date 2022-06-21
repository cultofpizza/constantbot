using ConstantBotApplication.Domain;
using ConstantBotApplication.Modals;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Interactions
{
    [EnabledInDm(false)]
    public class ReportModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BotContext context;

        public ReportModule(BotContext context)
        {
            this.context = context;
        }

        [SlashCommand("report","Creates report for moderators")]
        public async Task Report()
        {
            if(await context.GuildSettings.Where(i => i.GuilId == Context.Guild.Id && i.ReportChannelId != null).CountAsync() == 0)
            {
                await RespondAsync("Reporting not enabled on this server", ephemeral: true);
                return;
            }
            await RespondWithModalAsync<ReportModal>("channel_report");
        }

        [ModalInteraction("channel_report")]
        public async Task SaveReport(ReportModal modal)
        {
            var channel = Context.Guild.GetTextChannel((await context.GuildSettings.Where(i => i.GuilId == Context.Guild.Id && i.ReportChannelId != null).FirstAsync()).ReportChannelId.Value);

            var builder = new EmbedBuilder();
            builder.WithAuthor(Context.User)
                .WithCurrentTimestamp()
                .WithColor(Color.Blue)
                .WithFooter($"ID: {Context.User.Id}")
                .AddField("Channel",Context.Channel.Name)
                .WithTitle("Report")
                .WithDescription(modal.Content);

            await channel.SendMessageAsync(embed: builder.Build());

            await RespondAsync("Your report submitted successfuly",ephemeral: true);
        }
        
    }
}
