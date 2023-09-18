//using ConstantBotApplication.Domain;
//using ConstantBotApplication.Handlers;
using ConstantBotApplication.Extensions;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;
using static DSharpPlus.DiscordIntents;

namespace ConstantBotApplication
{
    public class Program
    {
        private DiscordClient _client;

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .WriteTo.Console()             
              .CreateLogger();

            var logFactory = new LoggerFactory().AddSerilog();

            var _config = new DiscordConfiguration
            {
                Intents = Guilds | GuildMembers | GuildBans | GuildVoiceStates | GuildMessages | GuildMessageReactions | DirectMessages | DirectMessageReactions,
                Token = Environment.GetEnvironmentVariable("token"),
                TokenType = TokenType.Bot,
                LoggerFactory = logFactory
            };

            _client = new DiscordClient(_config);

            var services = new Startup(client: _client).BuildServiceProvider();

            //var _interactionService = new InteractionService(_client);
            _client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            }).RegisterCommands(Assembly.GetEntryAssembly());


            _client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1)
            });

            await _client.ConnectAsync();

            _client.Ready += async (client, args) =>
            {
                //    await services.GetService<CommandHandler>().InitializeAsync();
                //    await services.GetService<InteractionsHandler>().InitializeAsync();

                services.GetService<Handlers.EventHandler>().RegisterEvents();
                await services.InitializeBotContextAsync();
                //    await services.InitializeVoiceManagmentAsync();
            };

        await Task.Delay(-1);
        }
    }
}
