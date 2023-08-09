using MacroBot.Core.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MacroBot.Core.DataAccess.Interceptors;

public class SaveChangesInterceptor : ISaveChangesInterceptor
{
    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context?.ChangeTracker.Entries() is null)
        {
            return ValueTask.FromResult(result);
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry is { Entity: BaseCreatedEntity baseCreatedEntity, State: EntityState.Added })
            {
                baseCreatedEntity.CreatedTimestamp = DateTime.Now;
            }
            
            if (entry is { Entity: BaseCreatedUpdatedEntity baseCreatedUpdatedEntity, State: EntityState.Modified })
            {
                baseCreatedUpdatedEntity.UpdatedTimestamp = DateTime.Now;
            }
        }

        return ValueTask.FromResult(result);
    }
}