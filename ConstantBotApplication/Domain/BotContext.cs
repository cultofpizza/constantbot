using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConstantBotApplication.Domain;

public class BotContext : DbContext
{
    public DbSet<Guild> Guilds { get; set; }
    public DbSet<SocialCounter> SocialCounters { get; set; }

    public BotContext(DbContextOptions options) : base(options)
    {
        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Guild>().HasKey(i => i.GuildId);

        modelBuilder.Entity<SocialCounter>().HasKey(i => new { i.GiverId, i.TakerId, i.Action });
    }
}
public class DesignBot : IDesignTimeDbContextFactory<BotContext>
{
    public BotContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<BotContext>();

        builder.UseNpgsql("User ID=postgres;Password=Const@ntTheBotPass93487;Host=178.20.47.47;Port=5432;Database=ConstantTheBot;");

        return new BotContext(builder.Options);
    }
}
