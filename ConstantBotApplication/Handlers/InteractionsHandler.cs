using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Handlers
{
	public class InteractionsHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly InteractionService _interactions;
		private readonly IServiceProvider _services;

		public InteractionsHandler(IServiceProvider services, InteractionService interactions, DiscordSocketClient client)
		{
			_interactions = interactions;
			_services = services;
			_client = client;
		}

		public async Task InitializeAsync()
		{
			await _interactions.AddModulesAsync(
				assembly: Assembly.GetEntryAssembly(),
				services: _services);
			await _interactions.RegisterCommandsGloballyAsync();
			_client.InteractionCreated += HandleInteractionsAsync;
		}

		private async Task HandleInteractionsAsync(SocketInteraction interaction)
		{
			var context = new SocketInteractionContext(_client, interaction);
			await _interactions.ExecuteCommandAsync(
				context: context,
				services: _services);
		}
	}
}
