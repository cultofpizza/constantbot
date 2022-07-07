using ConstantBotApplication.Voice;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Interactions;

[EnabledInDm(false)]
[DontAutoRegister]
[Group("legacy", "Don`t use it")]
public class MusicPlayerModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly VoiceClientManager manager;

    //public MusicPlayerModule(VoiceClientManager manager)
    //{
    //    this.manager = manager;
    //}

    [SlashCommand("play", "Play a track")]
    public async Task Play(string name)
    {
        var user = Context.User as SocketGuildUser;
        if (user == null) throw new Exception("Error while converting to guilduser");

        var channel = user.VoiceChannel;
        if (channel == null)
        {
            await RespondAsync("You need to be in voice channel");
            return;
        }
        await DeferAsync();

        if (!name.StartsWith("http"))
            name = name.Replace(':', ' ');


        var clientRes = await manager.GetClientAsync(channel);
        if (clientRes.Result == VoiceClientResult.ClientAlreadyInAnotherChannel)
        {
            await ModifyOriginalResponseAsync(m => m.Content = "Already connected to another channel");
            return;
        }
        else if (clientRes.Result == VoiceClientResult.Error)
        {
            throw clientRes.Exception ?? new Exception();
        }


        var track = await Track.GetTrackAsync(name);
        var client = clientRes.VoiceClient;
        client.Queue.Clear();
        client.Queue.AddLast(track);
        await client.Play();
        await ModifyOriginalResponseAsync(m => m.Content = $"Playing ``{track.Title}``");
    }

    [SlashCommand("stop", "Stops the player")]
    public async Task Stop()
    {
        if (manager.VoiceClients.TryGetValue(Context.Guild.Id, out var client))
        {
            await client.StopAsync();
            await RespondAsync("Stopped");
        }
        else
        {
            await RespondAsync("I am not playing music on this server");
        }
    }

    [SlashCommand("add", "Adds music to queue")]
    public async Task Add(string name)
    {
        if (manager.VoiceClients.TryGetValue(Context.Guild.Id, out var client))
        {
            await DeferAsync();
            var track = await Track.GetTrackAsync(name);
            client.Queue.AddLast(track);
            await ModifyOriginalResponseAsync(m => m.Content = $"Added ``{track.Title}`` to queue");
        }
        else
        {
            await RespondAsync("I am not playing music on this server");
        }
    }

    [SlashCommand("next", "Switches player to next track in queue")]
    public async Task Next()
    {
        if (manager.VoiceClients.TryGetValue(Context.Guild.Id, out var client))
        {
            if (client.Queue.Count == 0)
            {
                await RespondAsync("The queue is empty");
                return;
            }
            await DeferAsync();
            var title = client.Queue.First.Value.Title;
            await client.NextAsync();
            await ModifyOriginalResponseAsync(m => m.Content = $"Playing ``{title}``");
        }
        else
        {
            await RespondAsync("I am not playing music on this server");
        }
    }

    [SlashCommand("queue", "Show tracks in queue")]
    public async Task Queue()
    {
        if (manager.VoiceClients.TryGetValue(Context.Guild.Id, out var client))
        {
            var builder = new EmbedBuilder();
            string queue = string.Empty;
            var track = client.Queue.Count > 0 ? client.Queue.First : null;
            for (int i = 0; i < client.Queue.Count; i++)
            {
                queue += $"{i+1}. {track.Value.Title}\n";
                track = track.Next;
            }
            builder.WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Green)
                .WithTitle("Queue")
                .WithDescription(queue.Length > 0 ? queue : "Empty");

            await RespondAsync(embed: builder.Build());
        }
        else
        {
            await RespondAsync("I am not playing music on this server");
        }
    }
}
