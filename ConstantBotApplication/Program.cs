using ConstantBotApplication.Handlers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ConstantBotApplication
{
	public class Program
	{
        private DiscordSocketClient _client;

        public static Task Main(string[] args) => new Program().MainAsync();

		public async Task MainAsync()
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateLogger();

			var _config = new DiscordSocketConfig
			{
				MessageCacheSize = 100
			};

			_client = new DiscordSocketClient(_config);

			var services = new DIInitializer(client: _client).BuildServiceProvider();

			var _interactionService = new InteractionService(_client);

			_client.Log += Logger.LogAsync;
			await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token"));
			await _client.StartAsync();

            while (_client.ConnectionState != ConnectionState.Connected) { }

			await services.GetService<CommandHandler>().InitializeAsync();
			await services.GetService<InteractionsHandler>().InitializeAsync();

			await Task.Delay(-1);
		}
	}
}
