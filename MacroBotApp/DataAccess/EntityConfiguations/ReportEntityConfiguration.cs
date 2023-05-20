using MacroBot.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MacroBot.DataAccess.EntityConfiguations;

public class ReportEntityConfiguration : IEntityTypeConfiguration<ReportEntity>
{
	public void Configure(EntityTypeBuilder<ReportEntity> builder)
	{
		builder.ToTable("reports");
		builder.HasKey(e => e.Id);
		builder.Property(p => p.Id)
			.HasColumnName("id")
			.IsRequired();
		builder.Property(p => p.Reporter)
			.HasColumnName("reporter")
			.IsRequired();
		builder.Property(p => p.Content)
			.HasColumnName("content")
			.IsRequired();
		builder.Property(p => p.Guild)
			.HasColumnName("guild")
			.IsRequired();
		builder.Property(p => p.User)
			.HasColumnName("user")
			.IsRequired();
		builder.Property(p => p.Reported)
			.HasColumnName("reported")
			.IsRequired()
			.HasColumnType("datetime2");
	}
}