using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class LessonMapping : Profile
    {
        public LessonMapping()
        {
            // Map from Entity to DTO
            CreateMap<Lesson, LessonDTO>().ReverseMap();

            // Map from AddLessonDTO to Entity
            CreateMap<AddLessonDTO, Lesson>()
                .ForMember(dest => dest.OrderIndex, opt => opt.Ignore())
                .ForMember(dest => dest.Content, opt => opt.Ignore()); // Content is handled separately

            // Map from UpdateLessonDTO to Entity
            CreateMap<UpdateLessonDTO, Lesson>()
                .ForMember(dest => dest.OrderIndex, opt => opt.Ignore())
                .ForMember(dest => dest.ModuleId, opt => opt.Ignore())
                .ForMember(dest => dest.Content, opt => opt.Ignore()); // Content is handled separately
        }
    }
}
