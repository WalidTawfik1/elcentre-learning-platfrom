using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class CourseModuleMapping : Profile
    {
        public CourseModuleMapping()
        {
            CreateMap<AddCourseModuleDTO, CourseModule>();

            CreateMap<CourseModule, AddCourseModuleDTO>();

            CreateMap<UpdateCourseModuleDTO, CourseModule>()
                .ForMember(dest => dest.OrderIndex, opt => opt.Ignore());

            CreateMap<CourseModule, UpdateCourseModuleDTO>();
        }
    }
}
