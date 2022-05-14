using ConstantBotApplication.Handlers;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication
{
    internal class DIInitializer
    {
		private readonly CommandService _commands;
        private readonly InteractionService _interactions;
        private readonly DiscordSocketClient _client;

		// Ask if there are existing CommandService and DiscordSocketClient
		// instance. If there are, we retrieve them and add them to the
		// DI container; if not, we create our own.
		public DIInitializer(CommandService commands = null,InteractionService interactions = null, DiscordSocketClient client = null)
		{
			_commands = commands ?? new CommandService();
			_client = client ?? new DiscordSocketClient();
			_interactions = interactions ?? new InteractionService(_client);
		}

		public IServiceProvider BuildServiceProvider() => new ServiceCollection()
			.AddSingleton(_client)
			.AddSingleton(_commands)
			.AddSingleton(_interactions)
			.AddSingleton<CommandHandler>()
			.AddSingleton<InteractionsHandler>()
			.BuildServiceProvider();
	}
}
