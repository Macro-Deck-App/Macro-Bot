using MacroBot.Core.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MacroBot.Core.DataAccess.EntityConfigurations;

public class BaseCreatedUpdatedEntityConfig<T> : BaseCreatedEntityConfig<T>
    where T : BaseCreatedUpdatedEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.UpdatedTimestamp)
            .HasColumnName(ColumnPrefix + "updated_timestamp");
    }
}