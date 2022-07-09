using ConstantBotApplication.Domain;
using ConstantBotApplication.Voice;
using Discord;
using Discord.Audio;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace ConstantBotApplication.Modules.Interactions;

[EnabledInDm(false)]
[Group("music", "Music player")]
public class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly LavaNode<CustomLavaPlayer> lavaNode;
    private readonly BotContext context;
    private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

    public MusicModule(LavaNode<CustomLavaPlayer> lavaNode, BotContext context)
    {
        this.lavaNode = lavaNode;
        this.context = context;
    }

    [SlashCommand("play", "Plays music")]
    public async Task Play(string track, SearchType searchType = SearchType.YouTubeMusic)
    {
        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null)
        {
            await RespondAsync("You must be connected to a voice channel!");
            return;
        }

        if (lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            if (!player.IsConnected)
            {
                await lavaNode.LeaveAsync(player.VoiceChannel);
                player = await lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }
            else if (player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
            {
                await RespondAsync("Already connected to another channel");
                return;
            }

        }
        else player = await lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);

        var set = await context.GuildSettings.Where(i => i.GuilId == Context.Guild.Id).FirstAsync();
        if (set.Volume.HasValue) await player.UpdateVolumeAsync(set.Volume.Value);

        var searchResponse = await lavaNode.SearchAsync(searchType, track);

        string message = string.Empty;

        var builder = new EmbedBuilder()
            .WithColor(Color.Green);

        if (searchResponse.Status == SearchStatus.NoMatches)
        {
            await RespondAsync("No tracks found by your request");
            return;
        }
        else if (searchResponse.Status == SearchStatus.LoadFailed)
        {
            await RespondAsync("Track loading failed");
            return;
        }
        else if (searchResponse.Status == SearchStatus.PlaylistLoaded)
        {
            message = "Added tracks from playlist to queue";
            builder.AddField("Playlist", searchResponse.Playlist.Name);
            player.Queue.Enqueue(searchResponse.Tracks.Skip(1));
        }
        var lavaTrack = searchResponse.Tracks.First();

        await DeferAsync();

        await player.PlayAsync(lavaTrack);

        var artwork = await lavaTrack.FetchArtworkAsync();

        builder.WithTitle(lavaTrack.Title)
            .WithAuthor(lavaTrack.Author)
            .WithUrl(lavaTrack.Url)
            .WithImageUrl(artwork);

        await ModifyOriginalResponseAsync(m =>
        {
            if (message != string.Empty) m.Content = message;
            m.Embed = builder.Build();
        });

    }

    [SlashCommand("pause", "Pauses music")]
    public async Task Pause()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            await RespondAsync("I'm not playing any tracks.");
            return;
        }

        await player.PauseAsync();
        await RespondAsync("Paused");
    }

    [SlashCommand("resume", "Resumes music")]
    public async Task Resume()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Paused)
        {
            await RespondAsync("I'm not paused right now.");
            return;
        }

        await player.ResumeAsync();
        await RespondAsync("Resumed");
    }

    [SlashCommand("stop", "Stops music")]
    public async Task Stop()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
        {
            await RespondAsync("I'm not playing any tracks.");
            return;
        }

        await player.StopAsync();
        await RespondAsync("Stopped");
    }

    [SlashCommand("shuffle", "Shuffles the queue")]
    public async Task Shuffle()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        if (player.Queue.Count == 0)
        {
            await RespondAsync("I have tracks in queue.");
            return;
        }

        player.Queue.Shuffle();

        await RespondAsync("Tracks shuffled");
    }

    [SlashCommand("clear-queue", "Clears the queue")]
    public async Task Clear()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        if (player.Queue.Count == 0)
        {
            await RespondAsync("I have tracks in queue.");
            return;
        }

        player.Queue.Clear();

        await RespondAsync("Queue cleared");
    }

    [SlashCommand("skip", "Skips the track")]
    public async Task Skip()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        if (!player.Queue.TryDequeue(out var queueable))
        {
            await RespondAsync("Queue completed!");
            return;
        }
        if (!(queueable is LavaTrack track))
        {
            await RespondAsync("Next item in queue is not a track.");
            return;
        }

        await player.PlayAsync(track);

        var artwork = await track.FetchArtworkAsync();

        var builder = new EmbedBuilder()
            .WithColor(Color.Green)
            .WithAuthor(track.Author)
            .WithImageUrl(artwork)
            .WithUrl(track.Url)
            .WithDescription(track.Title);

        await RespondAsync(embed: builder.Build());
    }

    [SlashCommand("forward", "Forwards track")]
    public async Task Forward(int seconds)
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            await RespondAsync("I'm not playing any tracks.");
            return;
        }

        if (!player.Track.CanSeek)
        {
            await RespondAsync("This track isn`t seekable.");
            return;
        }

        TimeSpan timeSpan = player.Track.Position;
        timeSpan = timeSpan.Add(TimeSpan.FromSeconds(seconds));

        if (timeSpan > player.Track.Duration)
        {
            await RespondAsync("Too much for forwarding");
            return;
        }

        await player.SeekAsync(timeSpan);
        await RespondAsync("Position set to " + timeSpan);
    }

    [SlashCommand("backward", "Backwards track")]
    public async Task Backward(int seconds)
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            await RespondAsync("I'm not playing any tracks.");
            return;
        }

        if (!player.Track.CanSeek)
        {
            await RespondAsync("This track isn`t seekable.");
            return;
        }

        TimeSpan timeSpan = player.Track.Position;
        timeSpan = timeSpan.Subtract(TimeSpan.FromSeconds(seconds));

        if (timeSpan.Seconds < 0)
        {
            timeSpan = TimeSpan.FromSeconds(0);
        }

        await player.SeekAsync(timeSpan);
        await RespondAsync("Position set to " + timeSpan);
    }

    [RequireUserPermission(GuildPermission.Administrator)]
    [SlashCommand("volume", "Sets volume of player")]
    public async Task SetVolume(ushort volume)
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        await player.UpdateVolumeAsync(volume);
        await RespondAsync("Volume updated");

        var set = await context.GuildSettings.Where(i => i.GuilId == Context.Guild.Id).FirstAsync();
        set.Volume = volume;
        await context.SaveChangesAsync();
    }

    [SlashCommand("add", "Adds track to queue")]
    public async Task Add(string track, SearchType searchType = SearchType.YouTubeMusic)
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        var searchResponse = await lavaNode.SearchAsync(searchType, track);
        string message = string.Empty;

        var builder = new EmbedBuilder()
            .WithColor(Color.Green);

        if (searchResponse.Status == SearchStatus.NoMatches || searchResponse.Tracks.Count == 0)
        {
            await RespondAsync("No tracks found by your request");
            return;
        }
        else if (searchResponse.Status == SearchStatus.LoadFailed)
        {
            await RespondAsync("Track loading failed");
            return;
        }
        else if (searchResponse.Status == SearchStatus.PlaylistLoaded)
        {
            builder.AddField("Playlist", searchResponse.Playlist.Name);
            player.Queue.Enqueue(searchResponse.Tracks);

            message = "Added tracks from playlist to queue";
        }
        else
        {
            player.Queue.Enqueue(searchResponse.Tracks.First());
        }

        var lavaTrack = searchResponse.Tracks.First();
        var artwork = await lavaTrack.FetchArtworkAsync();

        builder.WithAuthor(lavaTrack.Author)
            .WithImageUrl(artwork)
            .WithUrl(lavaTrack.Url)
            .WithTitle(lavaTrack.Title);

        if (message == string.Empty)
            await RespondAsync(embed: builder.Build());
        else
            await RespondAsync(message, embed: builder.Build());
    }

    [SlashCommand("queue", "Shows queued tracks")]
    public async Task Queue()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        if (player.Queue.Count == 0)
        {
            await RespondAsync("I have no tracks in queue");
            return;
        }

        var builder = new EmbedBuilder();
        var queue = player.Queue.ToList();
        string queueString = string.Empty;

        for (int i = 0; i < queue.Count; i++)
        {
            queueString += $"{i + 1}. ``{queue[i].Author}`` - ``{queue[i].Title}``\n";
        }

        builder.WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
            .WithColor(Color.Green)
            .WithTitle("Queue")
            .WithDescription(queueString);

        await RespondAsync(embed: builder.Build());

    }

    [SlashCommand("lyrics", "(Beta) Gets lyrics from genius or OVH", runMode: RunMode.Async)]
    public async Task ShowGeniusLyrics(LyricsSource source = LyricsSource.Genius)
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            await RespondAsync("I'm not playing any tracks.");
            return;
        }
        string lyrics;
        try
        {
            if (source == LyricsSource.Genius)
                lyrics = await player.Track.FetchLyricsFromGeniusAsync();
            else
                lyrics = await player.Track.FetchLyricsFromOvhAsync();
        }
        catch
        {
            await RespondAsync($"Error occured while fetching lyrics for ``{player.Track.Title}``");
            return;
        }
        if (string.IsNullOrWhiteSpace(lyrics))
        {
            await RespondAsync($"No lyrics found for ``{player.Track.Title}``");
            return;
        }

        var splitLyrics = lyrics.Split('\n');
        var stringBuilder = new StringBuilder();
        foreach (var line in splitLyrics)
        {
            if (Range.Contains(stringBuilder.Length))
            {
                await RespondAsync($"Lyrics for ``{player.Track.Author} - {player.Track.Title}``\n ```{stringBuilder}```");
                stringBuilder.Clear();
            }
            else
            {
                stringBuilder.AppendLine(line);
            }
        }

        await RespondAsync($"Lyrics for ``{player.Track.Author} - {player.Track.Title}``\n ```{stringBuilder}```");
    }


    public enum LyricsSource
    {
        Genius,
        OVH
    }
}
