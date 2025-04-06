using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class Enrollments
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; }
        public DateTime? CompletionDate { get; set; }
        public float Progress { get; set; }
        public string StudentId { get; set; }
        public AppUser Student { get; set; }
        public int  CourseId { get; set; }
        public Courses Course { get; set; }
    }
}
