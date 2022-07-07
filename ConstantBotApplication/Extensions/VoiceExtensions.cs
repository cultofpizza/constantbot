using ConstantBotApplication.Voice;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace ConstantBotApplication.Extensions;

public static class VoiceExtensions
{

    public static IServiceCollection AddVoiceManagement(this IServiceCollection services)
    {
        string[] lavalinkConnectionStrings = Environment.GetEnvironmentVariable("lavalink").Split(';');

        var options = new LavaConfig
        {
            Hostname = lavalinkConnectionStrings[0].Split(':')[0],
            Authorization = lavalinkConnectionStrings[1],
            IsSsl = false,
            EnableResume = true,
            Port =  ushort.Parse(lavalinkConnectionStrings[0].Split(':')[1])
        };

        services.AddSingleton(options)
            .AddSingleton<LavaNode>();
        
        return services;
    }

    public static async Task<IServiceProvider> InitializeVoiceManagmentAsync(this IServiceProvider services)
    {
        var audioService = services.GetRequiredService<LavaNode>();
        audioService.OnLog += Logger.LogAsync;
        audioService.OnTrackEnded += OnTrackEnded;
        await audioService.ConnectAsync();


        return services;
    }

    private static async Task OnTrackEnded(TrackEndedEventArgs args)
    {
        if (args.Reason == TrackEndReason.Replaced) return;

        if (args.Player.Queue.Count==0)
        {
            return;
        }

        var player = args.Player;
        if (!player.Queue.TryDequeue(out var queueable))
        {
            await player.TextChannel.SendMessageAsync("Queue completed! Please add more tracks to rock n' roll!");
            return;
        }

        if (!(queueable is LavaTrack track))
        {
            await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
            return;
        }

        await args.Player.PlayAsync(track);


        var artwork = await player.Track.FetchArtworkAsync();

        var builder = new EmbedBuilder()
            .WithColor(Color.Green)
            .WithAuthor(player.Track.Author)
            .WithImageUrl(artwork)
            .WithUrl(player.Track.Url)
            .WithTitle(player.Track.Title);

        await args.Player.TextChannel.SendMessageAsync("Switching to next track in queue.\nNow playing" ,embed: builder.Build());
    }
}
