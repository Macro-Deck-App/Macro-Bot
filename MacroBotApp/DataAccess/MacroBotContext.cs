using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Reflection;
using MacroBot.DataAccess.Entities;
using MacroBot.DataAccess.EntityConfiguations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MacroBot.DataAccess;

public class MacroBotContext : DbContext
{
    public DbSet<TagEntity> TagEntities => Set<TagEntity>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (options.IsConfigured) return;
        var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = Constants.DatabasePath };
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