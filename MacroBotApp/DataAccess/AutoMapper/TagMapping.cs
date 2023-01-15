using AutoMapper;
using MacroBot.DataAccess.Entities;
using MacroBot.Models;

namespace MacroBot.DataAccess.AutoMapper;

public class TagMapping : Profile
{
    public TagMapping()
    {
        CreateMap<TagEntity, Tag>();
    }
}