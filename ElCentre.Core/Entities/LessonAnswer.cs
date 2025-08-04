using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class LessonAnswer
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public string Answer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public string CreatedById { get; set; }
        [Required]
        public string CreatedByName { get; set; }
        public string CreatorImage { get; set; }
        public bool IsInstructor { get; set; } = false;
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; } = null;
        public int HelpfulCount { get; set; } = 0;


        [Required]
        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public LessonQuestion Question { get; set; }



    }
}
