using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class EnrollmentMapping:Profile
    {
        public EnrollmentMapping()
        {
            CreateMap<Enrollment, EnrollmentDTO>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
                .ReverseMap();
        }
    }
}
