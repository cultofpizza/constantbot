using Discord;
using Discord.Audio;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Interactions;

[EnabledInDm(false)]
[DontAutoRegister]
[Obsolete("Use MusicPlayerModule instead")]
[Group("music","Obsolete")]
public class MusicModule : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("play", "(BETA) Plays music from not only youtube", runMode: RunMode.Async)]
    public async Task Play(string track)
    { 
        var user = Context.User as SocketGuildUser;
        if (user == null) throw new Exception("Error while converting to guilduser");

        var channel = user.VoiceChannel;
        if (channel == null)
        {
            await RespondAsync("You need to be in voice channel");
            return;
        }

        track = track.Replace(':', ' ');

        await DeferAsync();

        var client = await channel.ConnectAsync();
        string url, title;
        (url,title) = await GetUrlAndTitleAsync(track);
        using (var ffmpeg = CreateStream(url))
        using (var output = ffmpeg.StandardOutput.BaseStream)
        using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
        {
            try 
            {
                await ModifyOriginalResponseAsync(m => m.Content = $"Playing ``{title}``");
                await output.CopyToAsync(discord); 
            }
            finally { await discord.FlushAsync(); }
        }
    }

    private Process CreateStream(string url)
    {
        var playInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -i \"{url}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        var playProcess = Process.Start(playInfo);

        return playProcess;
    }

    private async Task<(string,string)> GetUrlAndTitleAsync(string path)
    {
        var searchInfo = new ProcessStartInfo
        {
            FileName = "youtube-dl",
            Arguments = $"-g -e -x --audio-format best --audio-quality 0 --default-search \"ytsearch\" \"{path}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        };

        var searchprocess = Process.Start(searchInfo);

        await searchprocess.WaitForExitAsync();
        var output = searchprocess.StandardOutput.ReadToEnd().Split('\n').Where(i => i.Length > 0).ToList();
        var title = output.First();
        var url = output.Last();
        return (url,title);
    }
}
