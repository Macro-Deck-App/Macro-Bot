using AutoMapper;
using MacroBot.Core.DataAccess.Entities;
using MacroBot.Core.Discord.Modules.Tagging;

namespace MacroBot.AutoMapper;

public class TagMapping : Profile
{
    public TagMapping()
    {
        CreateMap<TagEntity, Tag>()
            .ForMember(dest => dest.LastEdited, opt => opt.MapFrom(x => x.UpdatedTimestamp))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(x => x.CreatedTimestamp));
    }
}