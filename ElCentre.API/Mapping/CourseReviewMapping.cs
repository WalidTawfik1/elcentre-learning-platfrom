using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class CourseReviewMapping:Profile
    {
        public CourseReviewMapping()
        {
            CreateMap<CourseReview, CourseReviewDTO>().ReverseMap();
            CreateMap<CourseReview, ReturnCourseReviewDTO>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.User.Id))
                .ReverseMap();
            CreateMap<UpdateReviewDTO, CourseReview>().ReverseMap();
        }
    }
}
