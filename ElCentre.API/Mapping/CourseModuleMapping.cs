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

            // If you want to include Lessons in the CourseModuleDTO, uncomment the following code
            /*
            CreateMap<CourseModule, CourseModuleDTO>()
               .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons));

            CreateMap<Lesson, LessonsDTO>();*/

            //// Map Course entity to CourseDto
            //CreateMap<Course, CourseDto>()
            //    .ForMember(dest => dest.Instructor, opt => opt.MapFrom(src => src.Instructor));

            //// Map AppUser entity to UserDto
            //CreateMap<AppUser, UserDto>()
            //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            //// Complete mapping from CourseModule to CourseModuleDTO with nested objects
            //CreateMap<CourseModule, CourseModuleDTO>()
            //    .ForMember(dest => dest.Course, opt => opt.MapFrom(src => src.Course));

        }
    }
}
