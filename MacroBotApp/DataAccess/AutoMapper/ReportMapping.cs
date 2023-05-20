using AutoMapper;
using MacroBot.DataAccess.Entities;
using MacroBot.Discord.Modules.Reports;

namespace MacroBot.DataAccess.AutoMapper;

public class ReportMapping : Profile
{
	public ReportMapping()
	{
		CreateMap<ReportEntity, Report>();
	}
}