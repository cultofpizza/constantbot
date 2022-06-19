using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.Interactions;

public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly JsonSerializerOptions _jsonOptions;

    public RequestOptions DeletionRequestOptions
    {
        get
        {
            var options = RequestOptions.Default;
            options.AuditLogReason = $"Deletion by {Context.User.Username}";
            return options;
        }
    }


    public ModerationModule(JsonSerializerOptions jsonOptions)
    {
        _jsonOptions = jsonOptions;
    }

    [RequireUserPermission(GuildPermission.ManageMessages)]
    [SlashCommand("clear", "Clears chat messages")]
    public async Task Clear(int count = 1, SocketGuildUser user = null, int maxMessages = 100)
    {
        if (maxMessages < count) maxMessages = count;
        await DeferAsync();
        var messages = await Context.Channel.GetMessagesAsync(maxMessages).FlattenAsync();
        messages = messages.Where(i => i.Timestamp > DateTime.UtcNow.AddDays(-14)).AsEnumerable();
        if (user != null)
            messages = messages.Where(i => i.Author.Id == user.Id).AsEnumerable();
        messages = messages.Where(i => !(i.Flags.HasValue && i.Flags.Value.HasFlag(MessageFlags.Ephemeral))).Take(count).AsEnumerable();
        await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages, DeletionRequestOptions);
        await ModifyOriginalResponseAsync(m => m.Content = $"Deleted {messages.Count()} messages");
        await Task.Delay(5000);
        await DeleteOriginalResponseAsync();
    }

    [RequireUserPermission(GuildPermission.ManageMessages)]
    [MessageCommand("Delete after this")]
    public async Task ClearBelow(IMessage message)
    {
        var timeDelta = DateTimeOffset.UtcNow - message.Timestamp;
        if (timeDelta > TimeSpan.FromDays(14))
        {
            await RespondAsync($"{Emoji.Parse(":older_man:")} Message is too old for this", ephemeral: true);
        }
        else
        {
            await DeferAsync();
            var messages = (await Context.Channel.GetMessagesAsync(message, Direction.After).FlattenAsync()).ToList();
            messages = messages.Where(i => !(i.Flags.HasValue && i.Flags.Value.HasFlag(MessageFlags.Ephemeral))).ToList();
            messages.Add(message);
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages, DeletionRequestOptions);
            await ModifyOriginalResponseAsync(m => m.Content = $"Deleted {messages.Count()} messages");
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
        }
    }

    [RequireUserPermission(GuildPermission.ViewAuditLog)]
    [SlashCommand("audit", "Returns last audit actions")]
    public async Task Audit(int count = 1)
    {
        await DeferAsync();
        var audit = await Context.Guild.GetAuditLogsAsync(count).FlattenAsync();

        var auditJson = JsonSerializer.Serialize(audit, _jsonOptions);

        string filename = $"audit-{Context.Guild.Name}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.json";
        var stream = File.CreateText(filename);
        await stream.WriteAsync(auditJson);
        stream.Close();

        var files = new List<FileAttachment> { new FileAttachment(filename) };

        await ModifyOriginalResponseAsync(m =>
        {
            m.Attachments = files;
            m.Content = "Here is your audit";
        });

        await Task.Delay(5000);

        File.Delete(filename);
    }
}
