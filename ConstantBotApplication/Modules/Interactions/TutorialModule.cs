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

        [RequireOwner]
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

        [RequireOwner]
        [MessageCommand("Like")]
        public async Task Test(IMessage message)
        {
            Emoji.TryParse(":heart:", out var emote);
            await message.AddReactionAsync(emote);
            await RespondAsync("Like", ephemeral: true);
        }

        [RequireOwner]
        [SlashCommand("hueta","Requires edit")]
        public async Task Hueta()
        {
            var builder = new EmbedBuilder()
                .WithImageUrl(@"https://c.tenor.com/i_2G6W4VAewAAAAd/%D0%BF%D1%80%D0%BE%D1%88%D1%83-%D0%B4%D0%BE%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B0%D1%82%D1%8C.gif");
            await RespondAsync(embed: builder.Build());
        }

        [RequireOwner]
        [SlashCommand("vote-ban","Starts voting for ban")]
        public async Task VoteBan(int count)
        {
            var options = new List<SelectMenuOptionBuilder>();
            var users = (await Context.Guild.GetUsersAsync().FlattenAsync()).ToArray();
            Random random = new Random();
            IGuildUser user;
            for (int i = 0; i < count; i++)
            {
                user = users[random.Next(0, users.Length)];
                options.Add(new SelectMenuOptionBuilder().WithLabel(user.Username).WithValue(user.Id.ToString()));
            }
            var builder = new ComponentBuilder()
                .WithSelectMenu("vote-ban",options);

            await RespondAsync(components: builder.Build());
        }

        [RequireOwner]
        [ComponentInteraction("vote-ban")]
        public async Task AddVoteForBan(string item)
        {
            IUser user = await Context.Client.GetUserAsync(Convert.ToUInt64(item));

            var builder = new ComponentBuilder()
                .WithButton($"Ban {user.Username}!", $"vote-for-ban-{item}-0");

            await RespondAsync(components: builder.Build());
        }

        [RequireOwner]
        [ComponentInteraction("vote-for-ban-*-*")]
        public async Task VoteForBan(string id,string countStr)
        {
            int count = Convert.ToInt32(countStr);
            if (Convert.ToInt32(count)<5)
            {
                count++;
                await RespondAsync(Context.User.Username + "wants to ban!");
            }
            else
            {
                await RespondAsync(Context.User.Username + "wants to ban!\nBut I don`t have permission to do that");
            }
        }

        [RequireOwner]
        [SlashCommand("test", "Shows test modal")]
        public async Task Command()
        {
            var builder = new ComponentBuilder()
                .WithButton("Pizza","test-button")
                .Build();
            var mb = new ModalBuilder()
                .WithTitle("Fav Food")
                .WithCustomId("food_menu")
                .AddTextInput("What??", "food_name", placeholder: "Pizza")
                .AddTextInput("Why??", "food_reason", TextInputStyle.Paragraph, "TEST");

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        
    }
}
