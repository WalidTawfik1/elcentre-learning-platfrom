using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class CategoryMapping:Profile
    {
        public CategoryMapping()
        {
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Category, AddCategoryDTO>().ReverseMap();
        }
    }
}
