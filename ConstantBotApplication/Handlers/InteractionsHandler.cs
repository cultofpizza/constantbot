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
            //        foreach (var item in _client.Guilds)
            //        {
            //await _interactions.RegisterCommandsToGuildAsync(item.Id);
            //        }
            //_client.JoinedGuild += async guild => await _interactions.RegisterCommandsToGuildAsync(guild.Id);
            await _interactions.RegisterCommandsGloballyAsync();
            _interactions.InteractionExecuted += OnInteractionExecuted;
            _client.InteractionCreated += HandleInteractionsAsync;
		}

        private async Task OnInteractionExecuted(ICommandInfo cmd, IInteractionContext context, IResult result)
        {
			if (!result.IsSuccess)
			{
				if (result.Error == InteractionCommandError.UnmetPrecondition)
				{
					await context.Interaction.RespondAsync(Emoji.Parse(":octagonal_sign:") + " You are not allowed to perform this action!", ephemeral: true);
				}
                else
                {
					await Logger.LogAsync(new LogMessage(LogSeverity.Error,cmd.Name,result.ErrorReason));
					await context.Interaction.RespondAsync(Emoji.Parse(":cry:") + " Something went wrong", ephemeral: true);
                }
			}
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
