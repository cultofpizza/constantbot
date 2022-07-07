using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Voice;

public class VoiceClientManager
{
    public Dictionary<ulong, VoiceClient> VoiceClients { get; set; }
    private Dictionary<ulong, ushort> counter;

    public VoiceClientManager()
    {
        VoiceClients = new Dictionary<ulong, VoiceClient>();
        counter = new Dictionary<ulong, ushort>();

        new System.Threading.Thread(async () => await RunChecks()).Start();
    }

    public async Task<VoiceClientRequestResult> GetClientAsync(SocketVoiceChannel channel)
    {
        VoiceClient client;
        if (VoiceClients.ContainsKey(channel.Guild.Id))
        {
            client = VoiceClients[channel.Guild.Id];
            if (client.VoiceChannel.Id != channel.Id)
            {
                return new VoiceClientRequestResult(client, VoiceClientResult.ClientAlreadyInAnotherChannel);
            }
            return new VoiceClientRequestResult(client);
        }
        client = new VoiceClient();
        VoiceClients.Add(channel.Guild.Id, client);
        counter.Add(channel.Guild.Id, 0);
        await client.ConnectAsync(channel);
        return new VoiceClientRequestResult(client, VoiceClientResult.CreatedNewClient);
    }

    private async Task RunChecks()
    {
        while (true)
        {
            foreach (var item in counter)
            {
                if (VoiceClients.TryGetValue(item.Key, out var client))
                {
                    if (!client.IsPlaying)
                    {
                        counter[item.Key]++;
                        if (counter[item.Key] == 12)
                        {
                            VoiceClients[item.Key].Dispose();
                            VoiceClients.Remove(item.Key);
                        }
                    }
                    else
                        counter[item.Key] = 0;

                }
                else
                    counter.Remove(item.Key);
            }

            await Task.Delay(10000);
        }
    }
}
