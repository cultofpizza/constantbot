using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
                    await Logger.LogAsync(new LogMessage(LogSeverity.Error, cmd.Name, result.ErrorReason));
                    if (!context.Interaction.HasResponded)
                        await context.Interaction.RespondAsync(Emoji.Parse(":cry:") + " Something went wrong", ephemeral: true);
                    else
                        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content = Emoji.Parse(":cry:") + " Something went wrong");
                }
            }
            else
            {
                Log.Information($"Interaction \'{cmd.Name}\' completed successfuly");
            }
        }

        private async Task HandleInteractionsAsync(SocketInteraction interaction)
        {
            var context = new SocketInteractionContext(_client, interaction);
            if (interaction is SocketCommandBase)
            {
                var command = interaction as SocketCommandBase;
                var message = new { type = interaction.GetType().Name, ChannelName = command.Channel.Name, CommandName = command.CommandName };

                Log.Information("Interaction called");
                Log.Information(JsonSerializer.Serialize(message));

            }
            else if (interaction is SocketMessageComponent)
            {
                var component = interaction as SocketMessageComponent;
                var message = new { type = interaction.GetType().Name, ChannelName = interaction.Channel.Name, customId = component.Data.CustomId, componentType = component.Data.Type.ToString() };

                Log.Information("Interaction called");
                Log.Information(JsonSerializer.Serialize(message));
            }
            else if (interaction is SocketModal)
            {
                var component = interaction as SocketModal;
                var message = new { type = interaction.GetType().Name, ChannelName = interaction.Channel.Name, customId = component.Data.CustomId, modalType = component.Type.ToString() };

                Log.Information("Interaction called");
                Log.Information(JsonSerializer.Serialize(message));
            }
            else
            {
                var message = new { type = interaction.GetType().Name, ChannelName = interaction.Channel.Name };

                Log.Information("Interaction called");
                Log.Information(JsonSerializer.Serialize(message));
            }

            await _interactions.ExecuteCommandAsync(
                context: context,
                services: _services);
        }
    }
}
