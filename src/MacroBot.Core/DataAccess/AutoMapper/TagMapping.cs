using AutoMapper;
using MacroBot.Core.DataAccess.Entities;
using MacroBot.Core.Discord.Modules.Tagging;

namespace MacroBot.Core.DataAccess.AutoMapper;

public class TagMapping : Profile
{
    public TagMapping()
    {
        CreateMap<TagEntity, Tag>();
    }
}