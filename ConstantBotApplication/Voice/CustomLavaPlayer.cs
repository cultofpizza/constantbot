﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Player;
using Victoria.WebSocket;

namespace ConstantBotApplication.Voice;

public class CustomLavaPlayer : LavaPlayer
{
    public readonly ILogger<CustomLavaPlayer> logger;

    public IUserMessage ChatPlayer { get; set; }

    public int NotPlayingConter { get; set; } = 0;

    public CustomLavaPlayer(WebSocketClient lavaSocket, IVoiceChannel voiceChannel, ITextChannel textChannel, ILogger<CustomLavaPlayer> logger) : base(lavaSocket, voiceChannel, textChannel)
    {
        this.logger = logger;
    }
    public CustomLavaPlayer(WebSocketClient lavaSocket, IVoiceChannel voiceChannel, ITextChannel textChannel) : base(lavaSocket, voiceChannel, textChannel) { }

    public async Task RedrawPlayerAsync()
    {
        var embedBuilder = new EmbedBuilder();
        if (Track != null)
            embedBuilder
            .WithColor(PlayerState == PlayerState.Playing ? Color.Green : Color.Orange)
            .WithTitle(PlayerState.ToString() + " " + Track.Title)
            .WithAuthor(Track.Author)
            .WithUrl(Track.Url)
            .WithImageUrl(await Track.FetchArtworkAsync());
        else
            embedBuilder
                .WithColor(Color.Orange)
                .WithTitle(PlayerState.ToString());

        var queueEmbedBuilder = new EmbedBuilder();
        var queue = Vueue.ToList();
        string queueString = string.Empty;

        for (int i = 0; i < queue.Count; i++)
        {
            queueString += $"{i + 1}. ``{queue[i].Author}`` - ``{queue[i].Title}``\n";
        }

        queueEmbedBuilder.WithColor(Color.Green)
            .WithTitle("Queue")
            .WithDescription(queueString != string.Empty ? queueString : "Queue is empty");

        var componentBuilder = new ComponentBuilder();
        if (PlayerState == PlayerState.Playing)
            componentBuilder.WithButton("Pause", "pause", ButtonStyle.Primary, Emoji.Parse(":pause_button:"));
        else if (PlayerState == PlayerState.Paused)
            componentBuilder.WithButton("Resume", "resume", ButtonStyle.Primary, Emoji.Parse(":arrow_forward:"));
        else
            componentBuilder.WithButton("Resume", "resume", ButtonStyle.Primary, Emoji.Parse(":arrow_forward:"), disabled: true);

        componentBuilder
        .WithButton("Skip", "skip", ButtonStyle.Primary, Emoji.Parse(":track_next:"), disabled: !(Vueue.Count > 0))
        .WithButton("Stop", "stop", ButtonStyle.Primary, Emoji.Parse(":stop_button:"), disabled: PlayerState == PlayerState.Stopped || PlayerState == PlayerState.None)
        .WithButton("Add", "add", ButtonStyle.Primary, Emoji.Parse(":musical_note:"))
        .WithButton("Shuffle", "shuffle", ButtonStyle.Primary, Emoji.Parse(":twisted_rightwards_arrows:"), row: 1, disabled: !(Vueue.Count > 0))
        .WithButton("Clear", "clear", ButtonStyle.Primary, Emoji.Parse(":wastebasket:"), row: 1, disabled: !(Vueue.Count > 0))
        .WithButton("Refresh", "refresh", ButtonStyle.Primary, Emoji.Parse(":arrows_counterclockwise:"), row: 1)
        .WithButton("Leave", "leave", ButtonStyle.Danger, Emoji.Parse(":door:"), row: 1);

        if (ChatPlayer != null)
            await ChatPlayer.ModifyAsync(m =>
            {
                m.Embeds = new Embed[] { embedBuilder.Build(), queueEmbedBuilder.Build() };
                m.Components = componentBuilder.Build();
            });
        else
        {
            ChatPlayer = await TextChannel.SendMessageAsync(embeds: new Embed[] { embedBuilder.Build(), queueEmbedBuilder.Build() }, components: componentBuilder.Build()) as SocketUserMessage;
        }
    }
}
