using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstantBotApplication.Domain;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace ConstantBotApplication.Modules.SlashCommands;

public class SocialModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BotContext _context;
    private readonly Random _random;

    public SocialModule(BotContext context)
    {
        _context = context;
        _random = new Random();
    }

    [SlashCommand("cookie", "Give your friend a cookie")]
    public Task Cookie(SocketGuildUser user) => Social(SocialActionType.Cookies, Context.User as SocketGuildUser, user);

    [SlashCommand("slap", "Give your friend a slap")]
    public Task Slap(SocketGuildUser user) => Social(SocialActionType.Slaps, Context.User as SocketGuildUser, user);

    [SlashCommand("hug", "Give your friend a hug")]
    public Task Hug(SocketGuildUser user) => Social(SocialActionType.Hugs, Context.User as SocketGuildUser, user);

    [SlashCommand("stats", "Shows how many social actions were performed")]
    public async Task Stats(SocialActionType action, IUser user = null)
    {
        await DeferAsync();

        if (user == null)
            user = Context.User;

        var stats = await GetStats(user.Id, action);
        var users = new Dictionary<ulong, IUser>();

        var given = new Dictionary<IUser, int>();
        var taken = new Dictionary<IUser, int>();

        foreach (var item in stats)
        {
            if (!users.ContainsKey(item.TakerId))
                users.Add(item.TakerId, await Context.Client.GetUserAsync(item.TakerId));
            if (!users.ContainsKey(item.GiverId))
                users.Add(item.GiverId, await Context.Client.GetUserAsync(item.GiverId));
            if (item.GiverId == user.Id)
                given.Add(users[item.TakerId], item.Count);
            if (item.TakerId == user.Id)
                taken.Add(users[item.GiverId], item.Count);
        }

        string table = FormTable(given, taken, action);

        var builder = new EmbedBuilder()
            .WithAuthor(user)
            .WithTitle(action.ToString())
            .WithDescription(table);

        await ModifyOriginalResponseAsync(i => i.Embed = builder.Build());
    }

    [EnabledInDm(false)]
    [UserCommand("User Photo")]
    public async Task GetUserAvatar(IGuildUser user)
    {
        await RespondAsync(user.GetDisplayAvatarUrl(size: 4096));
    }

    private async Task<SocialCounter> GetOrCreateCounter(ulong giverId, ulong takerId, SocialActionType action)
    {
        var entity = await _context.SocialCounters.Where(i => i.GiverId == giverId && i.Action == action && i.TakerId == takerId).FirstOrDefaultAsync();
        if (entity == null)
        {
            entity = new SocialCounter { GiverId = giverId, TakerId = takerId, Action = action, Count = 0 };
            _context.SocialCounters.Add(entity);
            await _context.SaveChangesAsync();
        }
        return entity;
    }

    private async Task<List<SocialCounter>> GetStats(ulong userId, SocialActionType actionType)
    {
        var entity = await _context.SocialCounters.Where(i => (i.TakerId == userId || i.GiverId == userId) && i.Action == actionType).ToListAsync();
        return entity;
    }

    private string FormTable(Dictionary<IUser, int> given, Dictionary<IUser, int> taken, SocialActionType action)
    {
        string table = "```\n" +
            $"Given {action}\n" +
            "┌───────────────────────────┬───────┐\n" +
            "│ Member                    │ Count │\n" +
            "├───────────────────────────┼───────┤\n";

        if (given.Count == 0)
        {
            table += "│            None           │   -   │\n";

        }
        else
            foreach (var item in given.OrderByDescending(i => i.Value).Take(10).ToList())
            {
                StringBuilder tmp = new StringBuilder("│                           │       │\n");
                for (int j = 0; j < item.Key.Username.Length; j++)
                {
                    if (j + 2 >= 28)
                        break;
                    tmp[j + 2] = item.Key.Username[j];
                }
                var count = item.Value.ToString();
                for (int i = 30; i < 35; i++)
                {
                    if (i - 29 > count.Length) break;
                    tmp[i] = count[i - 30];
                }
                table += tmp.ToString();
            }

        table += "└───────────────────────────┴───────┘\n";

        if (given.Count > 10)
        {
            var extraGiven = given.Values.OrderByDescending(i => i).Skip(10);
            table += $"And {extraGiven.Sum()} more {action} to {extraGiven.Count()} other users.\n";
        }

        table += $"Taken {action}\n" +
            "┌───────────────────────────┬───────┐\n" +
            "│ Member                    │ Count │\n" +
            "├───────────────────────────┼───────┤\n";

        if (taken.Count == 0)
        {
            table += "│            None           │   -   │\n";

        }
        else
            foreach (var item in taken.OrderByDescending(i => i.Value).Take(10).ToList())
            {
                StringBuilder tmp = new StringBuilder("│                           │       │\n");
                for (int j = 0; j < item.Key.Username.Length; j++)
                {
                    if (j + 2 >= 28)
                        break;
                    tmp[j + 2] = item.Key.Username[j];
                }
                var count = item.Value.ToString();
                for (int i = 30; i < 35; i++)
                {
                    if (i - 29 > count.Length) break;
                    tmp[i] = count[i - 30];
                }
                table += tmp.ToString();
            }

        table += "└───────────────────────────┴───────┘\n";

        if (taken.Count > 10)
        {
            var extraTaken = taken.Values.OrderByDescending(i => i).Skip(10);
            table += $"And {extraTaken.Sum()} more {action} from {extraTaken.Count()} other users.\n";
        }

        table += "```";

        return table;
    }

    private async Task Social(SocialActionType actionType, SocketGuildUser giver, SocketGuildUser taker)
    {
        var counter = await GetOrCreateCounter(giver.Id, taker.Id, actionType);
        counter.Count++;

        string plural = actionType.ToString().ToLower();
        string singular = plural.Substring(0, plural.Length - 1);

        var attachments = await _context.SocialAttachments.Where(i => i.GuildId == Context.Guild.Id && i.Action == actionType).ToListAsync();

        if (attachments.Count == 0)
            await RespondAsync(string.Format($"You {action[(int)actionType]}! That's {counter.Count} {(IsPlural(counter.Count)?plural:singular)} now!", taker.Mention));
        else
        {
            var attachment = attachments[_random.Next(attachments.Count)];

            var builder = new EmbedBuilder()
                .WithTitle(string.Format($"You {action[(int)actionType]}", taker.DisplayName))
                .WithDescription(string.Format($"*{giver.Mention} {actionThirdPerson[(int)actionType]}*", taker.Mention))
                .WithImageUrl(attachment.Url)
                .WithFooter($"That's {counter.Count} {plural} now!");

            await RespondAsync(embed: builder.Build());
        }

        await _context.SaveChangesAsync();
    }

    private string[] action =
    {
        "gave {0} a cookie",
        "slap {0}",
        "hug {0}"
    };
    private string[] actionThirdPerson =
    {
        "gives {0} a cookie",
        "slaps {0}",
        "hugs {0}"
    };

    private bool IsPlural(int count)
    {
        string c = count.ToString();
        if (c[c.Length - 1] == '1')
            if(c.Length > 1 && c[c.Length - 2] == '1') return true;
            else return false;
        else return true;
    }
}
