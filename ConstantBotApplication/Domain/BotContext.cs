using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConstantBotApplication.Domain;

public class BotContext : DbContext
{
    public DbSet<GuildSettings> GuildSettings { get; set; }

    public BotContext(DbContextOptions options) : base(options) 
    { 
        Database.EnsureCreated();
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

        builder.UseFirebird("User=SYSDBA;Password=pizza;Database=ConstantTheBot.fdb;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;");

        return new BotContext(builder.Options);
    }
}
