using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConstantBotApplication.Voice;

public class VoiceClient : IDisposable
{
    public SocketVoiceChannel VoiceChannel { get; private set; }
    public LinkedList<Track> Queue { get; set; }

    private Thread thread;
    private Process ffmpegProcess;
    public bool IsPlaying { get; set; } = false;

    private IAudioClient _rawClient;

    public VoiceClient()
    {
        Queue = new LinkedList<Track>();
    }

    public async Task ConnectAsync(SocketVoiceChannel channel)
    {
        _rawClient = await channel.ConnectAsync();
        VoiceChannel = channel;
        _rawClient.Disconnected += async e => Dispose();
    }

    public async Task Play()
    {
        if (ffmpegProcess != null && !ffmpegProcess.HasExited)
            ffmpegProcess.Kill();
        thread = new Thread(async start => await PlayThread());
        thread.Start();
        IsPlaying = true;
    }

    public async Task StopAsync()
    {
        if (ffmpegProcess != null && !ffmpegProcess.HasExited)
        {
            ffmpegProcess.Kill();
            await ffmpegProcess.WaitForExitAsync();
            IsPlaying = false;
        }
    }

    public async Task NextAsync()
    {
        if (ffmpegProcess != null && !ffmpegProcess.HasExited)
        {
            ffmpegProcess.Kill();
        }
        else
        {
            await Play();
        }
        IsPlaying = true;
    }

    private async Task PlayThread()
    {
        while (Queue.Count != 0)
        {
            var playInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -i \"{Queue.First.Value.Url}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            ffmpegProcess = Process.Start(playInfo);

            using (var output = ffmpegProcess.StandardOutput.BaseStream)
            using (var discord = _rawClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {
                    Queue.RemoveFirst();
                    await output.CopyToAsync(discord);
                }
                finally { await discord.FlushAsync(); }
            }
            if (!IsPlaying) return;
        }
    }

    public void Dispose()
    {
        _rawClient.Dispose();
        ffmpegProcess?.Dispose();
    }

    public Task Pause()
    {
        throw new NotImplementedException();
    }

    public Task SetTrackTime()
    {
        throw new NotImplementedException();
    }
}
