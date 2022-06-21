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
					if (!context.Interaction.HasResponded)
						await context.Interaction.RespondAsync(Emoji.Parse(":cry:") + " Something went wrong", ephemeral: true);
					else
						await context.Interaction.ModifyOriginalResponseAsync(m => m.Content = Emoji.Parse(":cry:") + " Something went wrong");
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
