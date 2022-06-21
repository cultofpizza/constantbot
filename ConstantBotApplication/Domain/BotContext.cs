using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConstantBotApplication.Domain;

public class BotContext : DbContext
{
    public DbSet<GuildSettings> GuildSettings { get; set; }

    public BotContext(DbContextOptions options) : base(options) 
    { 
        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GuildSettings>().HasKey(i=>i.GuilId);
    }
}
public class DesignBot : IDesignTimeDbContextFactory<BotContext>
{
    public BotContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<BotContext>();

        builder.UseNpgsql("<connectionString>");

        return new BotContext(builder.Options);
    }
}
