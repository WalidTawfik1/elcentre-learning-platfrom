using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        public string ContentType { get; set; }

        public string Description { get; set; }

        public int OrderIndex { get; set; }

        public int DurationInMinutes { get; set; }

        public bool IsPublished { get; set; } = true;

        public bool IsPreview { get; set; } = false;

        [ForeignKey("Module")]
        public int ModuleId { get; set; }
        public CourseModule Module { get; set; }

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public ICollection<LessonQuestion> Questions { get; set; } = new List<LessonQuestion>();
    }
}
