using MacroBot.Core.DataAccess;
using MacroBot.Core.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MacroBot.Core.Extensions;

public static class MacroBotContextExtensions
{
    private static readonly ILogger Logger = Log.ForContext(typeof(MacroBotContextExtensions));

    public static IQueryable<T> GetNoTrackingSet<T>(this MacroBotContext context)
        where T : BaseEntity
    {
        return context.Set<T>().AsNoTracking();
    }

    public static async Task CreateAsync<T>(this MacroBotContext context, T obj)
        where T : BaseEntity
    {
        await context.AddAsync(obj);
        await context.SaveAsync();
    }

    public static async Task DeleteAsync<T>(this MacroBotContext context, T? existing)
        where T : BaseEntity
    {
        if (existing is not null)
        {
            context.Set<T>().Remove(existing);
        }

        await context.SaveAsync();
    }

    public static async Task UpdateAsync<T>(this MacroBotContext context, T obj)
        where T : BaseEntity
    {
        try
        {
            context.Entry(obj).State = EntityState.Modified;
            await context.SaveAsync();
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Failed to update entity {Entity}", nameof(T));
        }
        finally
        {
            context.Entry(obj).State = EntityState.Detached;
        }
    }

    private static async Task SaveAsync(this MacroBotContext context)
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Error while saving");
        }
        finally
        {
            context.ChangeTracker.Clear();
        }
    }
}