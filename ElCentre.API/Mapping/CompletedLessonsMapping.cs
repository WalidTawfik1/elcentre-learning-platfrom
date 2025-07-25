using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class CompletedLessonsMapping:Profile
    {
        public CompletedLessonsMapping()
        {
            CreateMap<CompletedLesson, CompletedLessonsDTO>().ReverseMap();
        }
    }
}
