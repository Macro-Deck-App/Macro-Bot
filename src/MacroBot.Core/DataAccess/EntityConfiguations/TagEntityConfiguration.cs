using MacroBot.Core.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MacroBot.Core.DataAccess.EntityConfiguations;

public class TagEntityConfiguration :  IEntityTypeConfiguration<TagEntity>
{

    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.ToTable("tags");   
        builder.HasKey(e => e.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();
        builder.Property(p => p.Author)
            .HasColumnName("author")
            .IsRequired();
        builder.Property(p => p.Content)
            .HasColumnName("content")
            .IsRequired();
        builder.Property(p => p.Guild)
            .HasColumnName("guild")
            .IsRequired();
        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired();
        builder.Property(p => p.LastEdited)
            .HasColumnName("last_edited")
            .IsRequired()
            .HasColumnType("datetime2");

    }
}