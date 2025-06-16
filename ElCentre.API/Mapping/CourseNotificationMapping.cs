using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class CourseNotificationMapping:Profile
    {
        public CourseNotificationMapping()
        {
            CreateMap<CourseNotificationDTO, CourseNotification>().ReverseMap();
        }
    }
}
