using ConstantBotApplication.Domain;
using ConstantBotApplication.Extensions;
using ConstantBotApplication.Handlers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;
using static Discord.GatewayIntents;

namespace ConstantBotApplication
{
    public class Program
    {
        private DiscordSocketClient _client;

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .CreateLogger();

            var _config = new DiscordSocketConfig
            {
                GatewayIntents = Guilds | GuildMembers | GuildBans | GuildVoiceStates | GuildMessages | GuildMessageReactions | DirectMessages | DirectMessageReactions,
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true
            };



            _client = new DiscordSocketClient(_config);

            var services = new Startup(client: _client).BuildServiceProvider();

            var _interactionService = new InteractionService(_client);
            Microsoft.Extensions.Logging.ILogger clientLogger = services.GetService<ILogger<DiscordSocketClient>>();
            _client.Log += message =>
            {
                switch (message.Severity)
                {
                    case LogSeverity.Critical:
                        clientLogger.LogCritical(message.Exception, message.Message);
                        break;
                    case LogSeverity.Error:
                        clientLogger.LogError(message.Exception, message.Message);
                        break;
                    case LogSeverity.Warning:
                        clientLogger.LogWarning(message.Exception, message.Message);
                        break;
                    case LogSeverity.Info:
                        clientLogger.LogInformation(message.Exception, message.Message);
                        break;
                    case LogSeverity.Verbose:
                        clientLogger.LogTrace(message.Exception, message.Message);
                        break;
                    case LogSeverity.Debug:
                        clientLogger.LogDebug(message.Exception, message.Message);
                        break;
                    default:
                        break;
                }
                return Task.CompletedTask;
            };
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token"));
            await _client.StartAsync();



            _client.Ready += async () =>
            {
                await services.GetService<CommandHandler>().InitializeAsync();
                await services.GetService<InteractionsHandler>().InitializeAsync();

                services.GetService<Handlers.EventHandler>().RegisterEvents();
                await services.InitializeBotContextAsync();
                await services.InitializeVoiceManagmentAsync();
            };

            await Task.Delay(-1);
        }
    }
}
