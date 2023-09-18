using Microsoft.Extensions.DependencyInjection;
using System;
using ConstantBotApplication.Extensions;
//using ConstantBotApplication.Voice;
using Microsoft.Extensions.Logging;
using Serilog;
using DSharpPlus;

namespace ConstantBotApplication
{
    internal class Startup
    {

        private readonly DiscordClient _client;

		public Startup(DiscordClient client)
		{
			_client = client;
		}

		public IServiceProvider BuildServiceProvider() => new ServiceCollection()
			.AddSingleton(_client)
			.AddBotContext()
			.AddDefaultJsonOptions()
			//.AddVoiceManagement()
			.AddLogging(loggingBuilder =>
				loggingBuilder.AddSerilog(dispose: true))
			.AddEventModules()
            .AddSingleton<Handlers.EventHandler>()
            .BuildServiceProvider();
	}
}
