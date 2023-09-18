using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modules.SlashCommands;

public class ModerationModule : ApplicationCommandModule
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ModerationModule(JsonSerializerOptions jsonOptions)
    {
        _jsonOptions = jsonOptions;
    }

    [SlashRequireUserPermissions(Permissions.ManageMessages)]
    [SlashCommand("clear", "Clears chat messages")]
    public async Task Clear(InteractionContext context, 
        [Option("Count","Messages to delete")]long count = 1, 
        [Option("User","Author of messages to clear")]DiscordUser user = null, 
        [Option("MaxMessages", "Limit of deleted messages")]long maxMessages = 100)
    {
        if (maxMessages < count) maxMessages = count;
        await context.DeferAsync();
        var messages = (await context.Channel.GetMessagesAsync((int)maxMessages)).ToList();
        messages = messages.Where(i => i.Timestamp > DateTime.UtcNow.AddDays(-14)).ToList();
        if (user != null)
            messages = messages.Where(i => i.Author.Id == user.Id).ToList();
        messages = messages.Where(i => !(i.Flags.HasValue && i.Flags.Value.HasFlag(MessageFlags.Ephemeral))).Take((int)count).ToList();
        await context.Channel.DeleteMessagesAsync(messages, $"Deletion by {context.User.Username} via clear command");
        await context.EditResponseAsync(new DiscordWebhookBuilder()
        {
            Content = $"Deleted {messages.Count()} messages"
        });
    }

    //[SlashRequireUserPermissions(Permissions.ManageMessages)]
    //[MessageCommand("Delete after this")]
    //public async Task ClearBelow(IMessage message)
    //{
    //    var timeDelta = DateTimeOffset.UtcNow - message.Timestamp;
    //    if (timeDelta > TimeSpan.FromDays(14))
    //    {
    //        await RespondAsync($"{Emoji.Parse(":older_man:")} Message is too old for this", ephemeral: true);
    //    }
    //    else
    //    {
    //        await DeferAsync(true);
    //        var messages = (await Context.Channel.GetMessagesAsync(message, Direction.After).FlattenAsync()).ToList();
    //        messages = messages.Where(i => !(i.Flags.HasValue && i.Flags.Value.HasFlag(MessageFlags.Ephemeral))).ToList();
    //        messages.Add(message);
    //        await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages, DeletionRequestOptions);
    //        await ModifyOriginalResponseAsync(m => m.Content = $"Deleted {messages.Count()} messages");
    //    }
    //}

    [SlashRequireUserPermissions(Permissions.ViewAuditLog)]
    [SlashCommand("audit", "Returns last audit actions")]
    public async Task Audit(InteractionContext context, 
        [Option("Count","Amount of audit events to fetch")]long count = 1)
    {
        await context.DeferAsync(true);
        var audit = await context.Guild.GetAuditLogsAsync((int)count);

        var auditJson = JsonSerializer.Serialize(audit, _jsonOptions);

        //string filename = $"audit-{context.Guild.Name}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.json";
        //var stream = File.CreateText(filename);
        //await stream.WriteAsync(auditJson);
        //stream.Close();
        //DiscordAttachment
        //var files = new List<DiscordAttachment> { new DiscordAttachment };
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(auditJson);
        writer.Flush();
        stream.Position = 0;
        var patch = new DiscordWebhookBuilder
        {
            Content = "Here is your audit"
        };
        patch.AddFile("audit.json", stream);
        await context.EditResponseAsync(patch);

        await Task.Delay(5000);

        //File.Delete(filename);
    }
}
