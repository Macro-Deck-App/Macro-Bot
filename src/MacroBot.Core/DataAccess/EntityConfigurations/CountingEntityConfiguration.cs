using MacroBot.Core.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MacroBot.Core.DataAccess.EntityConfigurations;

public class CountingEntityConfiguration :  BaseCreatedUpdatedEntityConfig<CountingEntity> {
    public CountingEntityConfiguration() {
        Table = "counting";
        ColumnPrefix = "cnt_";
    }

    public override void Configure(EntityTypeBuilder<CountingEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable(Table);

        builder.Property(p => p.CurrentAuthor)
            .HasColumnName(ColumnPrefix + "author")
            .IsRequired();

        builder.Property(p => p.CurrentCount)
            .HasColumnName(ColumnPrefix + "count")
            .IsRequired();

        builder.Property(p => p.HighScore)
            .HasColumnName(ColumnPrefix + "high_score")
            .IsRequired();
    }
}