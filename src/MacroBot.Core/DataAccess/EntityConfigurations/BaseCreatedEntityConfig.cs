using MacroBot.Core.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MacroBot.Core.DataAccess.EntityConfigurations;

public class BaseCreatedEntityConfig<T> : BaseEntityConfig<T>
    where T : BaseCreatedEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.CreatedTimestamp)
            .HasColumnName(ColumnPrefix + "created_timestamp")
            .IsRequired();
    }
}