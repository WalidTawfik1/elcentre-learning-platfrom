using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class QuizMapping:Profile
    {
        public QuizMapping()
        {
            CreateMap<Quiz, AddQuizDTO>().ReverseMap();
            CreateMap<Quiz, QuizDTO>().ReverseMap();
        }
    }
}
