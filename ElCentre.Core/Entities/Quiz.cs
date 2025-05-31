using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Question { get; set; }

        [Required]
        public string OptionA { get; set; }
        [Required]
        public string OptionB { get; set; }

        public string? OptionC { get; set; }

        public string? OptionD { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        public string? Explanation { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public Course Course { get; set; }


        [ForeignKey("Lesson")]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        [JsonIgnore]
        public ICollection<StudentQuiz> StudentQuizzes { get; set; } = new List<StudentQuiz>();


    }
}
