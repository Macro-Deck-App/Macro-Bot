using System.Reflection;
using MacroBot.Core.DataAccess.Entities;
using MacroBot.Core.DataAccess.EntityConfiguations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MacroBot.Core.DataAccess;

public class MacroBotContext : DbContext
{
    public DbSet<TagEntity> TagEntities => Set<TagEntity>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (options.IsConfigured) return;
        var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = Paths.DatabasePath };
        var connectionString = connectionStringBuilder.ToString();
        var connection = new SqliteConnection(connectionString); 
        var loggerFactory = new LoggerFactory()
            .AddSerilog();
        options.UseSqlite(connection,
            b => b.MigrationsAssembly(Assembly.GetExecutingAssembly()
                .GetName()
                .Name));
        options.UseLoggerFactory(loggerFactory);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TagEntityConfiguration());
    }
}