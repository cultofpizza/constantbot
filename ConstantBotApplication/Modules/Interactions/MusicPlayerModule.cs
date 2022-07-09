using ConstantBotApplication.Domain;
using ConstantBotApplication.Modals;
using ConstantBotApplication.Voice;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace ConstantBotApplication.Modules.Interactions;

[EnabledInDm(false)]
public class MusicPlayerModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly LavaNode<CustomLavaPlayer> lavaNode;
    private readonly BotContext context;

    public MusicPlayerModule(LavaNode<CustomLavaPlayer> lavaNode, BotContext context)
    {
        this.lavaNode = lavaNode;
        this.context = context;
    }

    [SlashCommand("player", "Displays chat player")]
    public async Task Player()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        await RespondWithPlayerAsync(player);

        if (player.ChatPlayer != null)
            await player.ChatPlayer.DeleteAsync();
        player.ChatPlayer = await GetOriginalResponseAsync();
    }

    private async Task RespondWithPlayerAsync(LavaPlayer player)
    {
        var embedBuilder = new EmbedBuilder();
        if (player.Track != null)
            embedBuilder
            .WithColor(player.PlayerState == PlayerState.Playing ? Color.Green : Color.Orange)
            .WithTitle(player.PlayerState.ToString() + " " + player.Track.Title)
            .WithAuthor(player.Track.Author)
            .WithUrl(player.Track.Url)
            .WithImageUrl(await player.Track.FetchArtworkAsync());
        else
            embedBuilder
                .WithColor(Color.Orange)
                .WithTitle(player.PlayerState.ToString());

        var queueEmbedBuilder = new EmbedBuilder();
        var queue = player.Queue.ToList();
        string queueString = string.Empty;

        for (int i = 0; i < queue.Count; i++)
        {
            queueString += $"{i + 1}. ``{queue[i].Author}`` - ``{queue[i].Title}``\n";
        }

        queueEmbedBuilder.WithColor(Color.Green)
            .WithTitle("Queue")
            .WithDescription(queueString != string.Empty ? queueString : "Queue is empty");

        var componentBuilder = new ComponentBuilder();
        if (player.PlayerState == PlayerState.Playing)
            componentBuilder.WithButton("Pause", "pause", ButtonStyle.Primary, Emoji.Parse(":pause_button:"));
        else if (player.PlayerState == PlayerState.Paused)
            componentBuilder.WithButton("Resume", "resume", ButtonStyle.Primary, Emoji.Parse(":arrow_forward:"));
        else
            componentBuilder.WithButton("Resume", "resume", ButtonStyle.Primary, Emoji.Parse(":arrow_forward:"), disabled: true);

        componentBuilder
        .WithButton("Skip", "skip", ButtonStyle.Primary, Emoji.Parse(":track_next:"), disabled: !(player.Queue.Count > 0))
        .WithButton("Stop", "stop", ButtonStyle.Primary, Emoji.Parse(":stop_button:"), disabled: player.PlayerState == PlayerState.Stopped || player.PlayerState == PlayerState.None)
        .WithButton("Add", "add", ButtonStyle.Primary, Emoji.Parse(":musical_note:"))
        .WithButton("Shuffle", "shuffle", ButtonStyle.Primary, Emoji.Parse(":twisted_rightwards_arrows:"), row: 1, disabled: !(player.Queue.Count > 0))
        .WithButton("Clear", "clear", ButtonStyle.Primary, Emoji.Parse(":wastebasket:"), row: 1, disabled: !(player.Queue.Count > 0))
        .WithButton("Refresh", "refresh", ButtonStyle.Primary, Emoji.Parse(":arrows_counterclockwise:"), row: 1)
        .WithButton("Leave", "leave", ButtonStyle.Danger, Emoji.Parse(":door:"), row: 1);
        await RespondAsync(embeds: new Embed[] { embedBuilder.Build(), queueEmbedBuilder.Build() }, components: componentBuilder.Build());
    }

    [SlashCommand("play", "Plays music and shows player")]
    public async Task Play(string track, SearchType searchType)
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
            player.Queue.Enqueue(searchResponse.Tracks.Skip(1));
        }
        var lavaTrack = searchResponse.Tracks.First();

        await player.PlayAsync(lavaTrack);

        await RespondWithPlayerAsync(player);

        if (player.ChatPlayer != null)
            await player.ChatPlayer.DeleteAsync();
        player.ChatPlayer = await GetOriginalResponseAsync();
    }

    [ComponentInteraction("pause")]
    public async Task Pause()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        var voiceState = Context.User as IVoiceState;
        if (voiceState == null || player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be connected to channel with me");
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            await RespondAsync("I'm not playing any tracks.");
            return;
        }

        await player.PauseAsync();
        await player.RedrawPlayerAsync();
        await RespondAsync();
    }

    [ComponentInteraction("resume")]
    public async Task Resume()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        var voiceState = Context.User as IVoiceState;
        if (voiceState == null || player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be connected to channel with me");
            return;
        }

        if (player.PlayerState != PlayerState.Paused)
        {
            await RespondAsync("I'm not paused right now.");
            return;
        }

        await player.ResumeAsync();
        await player.RedrawPlayerAsync();
        await RespondAsync();
    }

    [ComponentInteraction("skip")]
    public async Task Skip()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        var voiceState = Context.User as IVoiceState;
        if (voiceState == null || player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be connected to channel with me");
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
        await player.RedrawPlayerAsync();
        await RespondAsync();
    }

    [ComponentInteraction("stop")]
    public async Task Stop()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        var voiceState = Context.User as IVoiceState;
        if (voiceState == null || player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be connected to channel with me");
            return;
        }

        if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
        {
            await RespondAsync("I'm not playing any tracks.");
            return;
        }

        await player.StopAsync();
        await player.RedrawPlayerAsync();
        await RespondAsync();
    }

    [ComponentInteraction("add")]
    public async Task Add()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        var voiceState = Context.User as IVoiceState;
        if (voiceState == null || player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be connected to channel with me");
            return;
        }

        await player.RedrawPlayerAsync(true);
        await RespondAsync();
    }

    [ComponentInteraction("shuffle")]
    public async Task Shuffle()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        var voiceState = Context.User as IVoiceState;
        if (voiceState == null || player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be connected to channel with me");
            return;
        }
        if (player.Queue.Count == 0)
        {
            await RespondAsync("I have tracks in queue.");
            return;
        }

        player.Queue.Shuffle();
        await player.RedrawPlayerAsync();
        await RespondAsync();
    }

    [ComponentInteraction("refresh")]
    public async Task Refresh()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        await player.RedrawPlayerAsync();
        await RespondAsync();
    }

    [ComponentInteraction("clear")]
    public async Task Clear()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        var voiceState = Context.User as IVoiceState;
        if (voiceState == null || player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be connected to channel with me");
            return;
        }
        if (player.Queue.Count == 0)
        {
            await RespondAsync("I have tracks in queue.");
            return;
        }

        player.Queue.Clear();
        await player.RedrawPlayerAsync();
        await RespondAsync();
    }

    [ComponentInteraction("searchtype")]
    public async Task AddAfterSelecting(string value)
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }
        var voiceState = Context.User as IVoiceState;
        if (voiceState == null || player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be connected to channel with me");
            return;
        }

        var builder = new ModalBuilder()
            .WithTitle("Search")
            .WithCustomId("addby:" + value)
            .AddTextInput("Name or url of track", "track");

        await player.RedrawPlayerAsync();
        if (value != "cancel")
            await RespondWithModalAsync(builder.Build());
        else
            await RespondAsync();
    }

    [ModalInteraction("addby:yt-music")]
    public Task AddAfterTypingYtMusic(SearchModal modal) => AddToQueue(SearchType.YouTubeMusic, modal.Track);
    [ModalInteraction("addby:youtube")]
    public Task AddAfterTypingYt(SearchModal modal) => AddToQueue(SearchType.YouTube, modal.Track);
    [ModalInteraction("addby:soundcloud")]
    public Task AddAfterTypingSc(SearchModal modal) => AddToQueue(SearchType.SoundCloud, modal.Track);
    [ModalInteraction("addby:direct")]
    public Task AddAfterTypingDirect(SearchModal modal) => AddToQueue(SearchType.Direct, modal.Track);

    private async Task AddToQueue(SearchType searchType, string track)
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            return;
        }

        var searchResponse = await lavaNode.SearchAsync(searchType, track);

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
            player.Queue.Enqueue(searchResponse.Tracks);
        }
        else
        {
            player.Queue.Enqueue(searchResponse.Tracks.First());
        }

        await player.RedrawPlayerAsync();
        await RespondAsync();
    }

    [ComponentInteraction("leave")]
    public async Task Leave()
    {
        if (!lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await RespondAsync("I'm not connected to a voice channel.");
            await player.ChatPlayer.DeleteAsync();
            return;
        }
        var voiceState = Context.User as IVoiceState;
        if (voiceState == null || player.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be connected to channel with me");
            return;
        }

        await lavaNode.LeaveAsync(player.VoiceChannel);
        await player.ChatPlayer.DeleteAsync();
        await RespondAsync("Bye bye");
    }
}
