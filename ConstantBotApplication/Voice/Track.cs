﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Voice;

public class Track
{
    public string Url;
    public string Title;

    public static async Task<Track> GetTrackAsync(string path)
    {
        var searchInfo = new ProcessStartInfo
        {
            FileName = "youtube-dl",
            Arguments = $"-g -e -x --audio-format best --audio-quality 0 --default-search \"ytsearch\" \"{path}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        };

        List<string> output;
        string title, url;
        int retries = 0;
        while (true)
        {
            var searchprocess = Process.Start(searchInfo);

            await searchprocess.WaitForExitAsync();
            if (retries > 3) throw new ArgumentException("youtube-dl fails to get track");
            retries++;
            output = searchprocess.StandardOutput.ReadToEnd().Split('\n').Where(i => i.Length > 0).ToList();
            if (output.Count < 2) continue;
            title = output.First();
            url = output.Last();
            var client = new HttpClient();
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            if (response.IsSuccessStatusCode)
                break;

        }
        return new Track { Title = title, Url = url };
    }
}
