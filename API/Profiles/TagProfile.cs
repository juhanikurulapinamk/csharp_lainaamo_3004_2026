

using API.Dtos;
using API.Models;
using AutoMapper;

public class TagProfile : Profile
{
    public TagProfile()
    {
        CreateMap<Tag, TagDto>();
    }
}