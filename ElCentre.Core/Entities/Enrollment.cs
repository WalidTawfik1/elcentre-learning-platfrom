using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Active";

        public DateTime? CompletionDate { get; set; }

        public float Progress { get; set; } = 0;

        [ForeignKey("Student")]
        public string StudentId { get; set; }
        public AppUser Student { get; set; }

        [ForeignKey("Course")]
        public int  CourseId { get; set; }
        public Course Course { get; set; }

        public ICollection<Payment> Payments { get; set; }

    }
}
