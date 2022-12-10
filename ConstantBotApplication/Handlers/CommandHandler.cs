using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<CommandHandler> logger;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services, CommandService commands, DiscordSocketClient client, ILogger<CommandHandler> logger)
        {
            _commands = commands;
            _services = services;
            _client = client;
            this.logger = logger;
        }

        public async Task InitializeAsync()
        {
            // Pass the service provider to the second parameter of
            // AddModulesAsync to inject dependencies to all modules 
            // that may require them.
            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            // Don't process the command if it was a system message
            var message = msg as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);
            // Pass the service provider to the ExecuteAsync method for
            // precondition checks.

            _commands.CommandExecuted += OnCommandExecutedAsync;

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // We have access to the information of the command executed,
            // the context of the command, and the result returned from the
            // execution in this event.

            // We can tell the user what went wrong
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnmetPrecondition)
                {
                    await context.Channel.SendMessageAsync(Emoji.Parse(":octagonal_sign:") + " You are not allowed to perform this action!");
                }
                else if (result.Error == CommandError.UnknownCommand) { }
                else
                {
                    logger.LogError($"Command error: {result.ErrorReason}");
                }

            }
        }
    }
}
