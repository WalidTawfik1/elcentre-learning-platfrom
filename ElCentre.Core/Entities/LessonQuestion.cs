using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class LessonQuestion
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Question { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public string CreatedById { get; set; }
        [Required]
        public string CreatedByName { get; set; }
        public string CreatorImage { get; set; }
        public bool IsInstructor { get; set; } = false;
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; } = null;

        [Required]
        [ForeignKey("Lesson")]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public ICollection<LessonAnswer> Answers { get; set; } = new List<LessonAnswer>(); 
    }
}
