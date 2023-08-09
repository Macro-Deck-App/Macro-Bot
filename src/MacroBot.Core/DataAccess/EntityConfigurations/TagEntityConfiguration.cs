using MacroBot.Core.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MacroBot.Core.DataAccess.EntityConfigurations;

public class TagEntityConfiguration :  BaseCreatedUpdatedEntityConfig<TagEntity>
{
    public TagEntityConfiguration()
    {
        Table = "tags";
        ColumnPrefix = "t_";
    }

    public override void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        base.Configure(builder);
        
        builder.ToTable(Table);
        
        builder.Property(p => p.Author)
            .HasColumnName(ColumnPrefix + "author")
            .IsRequired();
        
        builder.Property(p => p.Content)
            .HasColumnName(ColumnPrefix + "content")
            .IsRequired();
        
        builder.Property(p => p.Guild)
            .HasColumnName(ColumnPrefix + "guild")
            .IsRequired();
        
        builder.Property(p => p.Name)
            .HasColumnName(ColumnPrefix + "name")
            .IsRequired();
    }
}