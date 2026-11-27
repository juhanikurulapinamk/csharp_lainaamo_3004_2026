using API.Dtos;
using API.Models;
using AutoMapper;

namespace API.Profiles
{
    public class ItemProfile : Profile
    {
        public ItemProfile()
        {
            // Id, Name ja Description mäpätään automaattisesti samannimisistä propertyista.
            //
            // CategoryName mäpätään myös automaattisesti: AutoMapper "litistää" (flattening)
            // nimen Category + Name -> i.Category.Name
            //
            // Category (CategoryDto) mäpätään CategoryProfilen
            // CreateMap<Category, CategoryDto>() -määrityksen avulla.
            //
            // CopyCount pitää määritellä itse, koska Itemillä ei ole
            // "Copy"-propertya (se on "Copies"), joten AutoMapper ei osaa päätellä sitä.
            CreateMap<Item, ItemDto>()
                .ForMember(dest => dest.CopyCount, opt => opt.MapFrom(src => src.Copies.Count));
        }
    }
}