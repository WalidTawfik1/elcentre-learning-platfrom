using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class CourseMapping:Profile
    {
        public CourseMapping()
        {
            CreateMap<Course, CourseDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.InstructorName,
                     opt => opt.MapFrom(src => $"{src.Instructor.FirstName} {src.Instructor.LastName}"))
                .ReverseMap();

            CreateMap<Course, AddCourseDTO>()
                .ForMember(destinationMember: t => t.Thumbnail, memberOptions: opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Course, UpdateCourseDTO>()
                .ForMember(destinationMember: t => t.Thumbnail, memberOptions: opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
