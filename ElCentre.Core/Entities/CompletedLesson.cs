using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class CompletedLesson
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public string StudentId { get; set; }
        public AppUser Student { get; set; }

        [ForeignKey("Lesson")]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        [ForeignKey("Enrollment")]
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CompletedDate { get; set; }
    }
}
