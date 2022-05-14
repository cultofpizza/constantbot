﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
		private readonly CommandService _commands;
		private readonly IServiceProvider _services;

		public CommandHandler(IServiceProvider services, CommandService commands, DiscordSocketClient client)
		{
			_commands = commands;
			_services = services;
			_client = client;
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
			await _commands.ExecuteAsync(
				context: context,
				argPos: argPos,
				services: _services);
			// ...
		}

		public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
		{
			// We have access to the information of the command executed,
			// the context of the command, and the result returned from the
			// execution in this event.
	
			// We can tell the user what went wrong
			if (!string.IsNullOrEmpty(result?.ErrorReason))
			{
			    await context.Channel.SendMessageAsync(result.ErrorReason);
			}
	
			// ...or even log the result (the method used should fit into
			// your existing log handler)
			var commandName = command.IsSpecified ? command.Value.Name : "A command";
			await Logger.LogAsync(new LogMessage(LogSeverity.Info, 
			    "CommandExecution", 
			    $"{commandName} was executed at {DateTime.UtcNow}."));
		}
	}
}