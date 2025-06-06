using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public class CourseReviewDTO
    {
        public int CourseId { get; set; }
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
    }

    public class ReturnCourseReviewDTO
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentImage { get; set; }
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Count { get; set; }
    }

    public class UpdateReviewDTO
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
    }
}
