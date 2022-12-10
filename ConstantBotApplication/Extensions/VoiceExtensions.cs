using ConstantBotApplication.Voice;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

namespace ConstantBotApplication.Extensions;

public static class VoiceExtensions
{

    public static IServiceCollection AddVoiceManagement(this IServiceCollection services)
    {
        string[] lavalinkConnectionStrings = Environment.GetEnvironmentVariable("lavalink").Split(';');
        var options = new NodeConfiguration
        {
            Hostname = lavalinkConnectionStrings[0].Split(':')[0],
            Authorization = lavalinkConnectionStrings[1],
            IsSecure = false,
            EnableResume = true,
            Port =  ushort.Parse(lavalinkConnectionStrings[0].Split(':')[1]),
            
        };
        
        services.AddSingleton(options)
            .AddSingleton<LavaNode<CustomLavaPlayer,LavaTrack>>();
        return services;
    }

    public static async Task<IServiceProvider> InitializeVoiceManagmentAsync(this IServiceProvider services)
    {
        var audioService = services.GetRequiredService<LavaNode<CustomLavaPlayer, LavaTrack>>();
        audioService.OnTrackEnd += OnTrackEnded;
        await audioService.ConnectAsync();

        var manager = new Thread(async _ => await PlayerManager(audioService));
        manager.Start();

        return services;
    }

    private static async Task OnTrackEnded(TrackEndEventArg<CustomLavaPlayer, LavaTrack> args)
    {
        args.Player.logger.LogInformation($"Track {args.Track.Title} ended");
        if (args.Reason == TrackEndReason.Replaced || args.Reason == TrackEndReason.Stopped) return;

        if (args.Player.Vueue.Count==0)
        {
            return;
        }

        var player = args.Player as CustomLavaPlayer;
        if (!player.Vueue.TryDequeue(out var queueable))
        {
            await player.RedrawPlayerAsync();
            return;
        }

        if (!(queueable is LavaTrack track))
        {
            await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
            return;
        }

        await args.Player.PlayAsync(track);


        var artwork = await player.Track.FetchArtworkAsync();

        await player.RedrawPlayerAsync();
    }

    public static async Task PlayerManager(LavaNode<CustomLavaPlayer, LavaTrack> lavaNode)
    {
        while (true)
        {
            foreach (var player in lavaNode.Players)
            {
                if (!player.IsConnected)
                {
                    await player.ChatPlayer.DeleteAsync();
                    await lavaNode.LeaveAsync(player.VoiceChannel);
                }
                else if (player.PlayerState != PlayerState.Playing)
                {
                    player.NotPlayingConter++;
                    if (player.NotPlayingConter > 10)
                    {
                        await player.ChatPlayer.DeleteAsync();
                        await lavaNode.LeaveAsync(player.VoiceChannel);
                    }
                }
                else
                {
                    player.NotPlayingConter = 0;
                }
            }

            await Task.Delay(30000);
        }
    }
}
