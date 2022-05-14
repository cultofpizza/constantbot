using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace ConstantBotApplication.Modules.Interactions
{
    public class TutorialModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("echo", "Echo an input")]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }

        [SlashCommand("annoy", "Annoys your friend")]
        public async Task Ping(IUser victim)
        {
            await RespondAsync(victim.Mention);
        }

        [RequireOwner]
        [SlashCommand("button", "Makes fancy button")]
        public async Task Button(string text)
        {
            var builder = new ComponentBuilder()
                .WithButton(text, "custom-id");

            await RespondAsync("Here is a button!", components: builder.Build());
        }

        [SlashCommand("congrats", "Makes your friend happy")]
        public async Task Congrats(IUser friend)
        {
            var builder = new EmbedBuilder()
                .WithImageUrl(@"https://c.tenor.com/ksp0bo9FBMkAAAAS/july4th-july-fourth.gif");
            await RespondAsync(friend.Mention, embed: builder.Build()) ;
        }

        [UserCommand("User Photo")]
        public async Task GetUserAvatar(IUser user)
        {
            var builder = new EmbedBuilder()
                .WithImageUrl(user.GetAvatarUrl(size: 4096));
            await RespondAsync(user.GetAvatarUrl(size: 4096));
        }

        [MessageCommand("Like")]
        public async Task Test(IMessage message)
        {
            Emoji.TryParse(":heart:", out var emote);
            await message.AddReactionAsync(emote);
            await RespondAsync("Like");
        }

        [SlashCommand("hueta","Requires edit")]
        public async Task Hueta()
        {
            var builder = new EmbedBuilder()
                .WithImageUrl(@"https://c.tenor.com/i_2G6W4VAewAAAAd/%D0%BF%D1%80%D0%BE%D1%88%D1%83-%D0%B4%D0%BE%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B0%D1%82%D1%8C.gif");
            await RespondAsync(embed: builder.Build());
        }
    }
}
