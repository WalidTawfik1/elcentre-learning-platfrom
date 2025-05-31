using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public record AddQuizDTO
    {
        public string Question { get; set; }

        public string OptionA { get; set; }

        public string OptionB { get; set; }

        public string? OptionC { get; set; }

        public string? OptionD { get; set; }

        public string CorrectAnswer { get; set; }

        public string? Explanation { get; set; }

        public int CourseId { get; set; }

        public int LessonId { get; set; }

    }
    public record QuizDTO:AddQuizDTO
    {
        public int Id { get; set; }
    }
}
