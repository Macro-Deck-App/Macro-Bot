using System.Reflection;
using MacroBot.Core.Config;
using MacroBot.Core.DataAccess.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MacroBot.Core.DataAccess;

public class MacroBotContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connectionString = MacroBotConfig.DatabaseConnectionString;
        var loggerFactory = new LoggerFactory()
            .AddSerilog();
        options.UseNpgsql(connectionString);
        options.UseLoggerFactory(loggerFactory);
        options.AddInterceptors(new SaveChangesInterceptor());
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("macro_bot");
        
        var applyGenericMethod =
            typeof(ModelBuilder).GetMethod("ApplyConfiguration", BindingFlags.Instance | BindingFlags.Public);
        
        var types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(c => c is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false });
        
        foreach (var type in types) 
        {
            foreach (var i in type.GetInterfaces())
            {
                if (!i.IsConstructedGenericType || i.GetGenericTypeDefinition() != typeof(IEntityTypeConfiguration<>))
                {
                    continue;
                }
                
                var applyConcreteMethod = applyGenericMethod?.MakeGenericMethod(i.GenericTypeArguments[0]);
                applyConcreteMethod?.Invoke(modelBuilder, new [] { Activator.CreateInstance(type) });
                break;
            }
        }
    }
}