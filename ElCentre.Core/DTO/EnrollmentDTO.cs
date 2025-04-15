using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public class EnrollmentDTO
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public float Progress { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
    }
}
