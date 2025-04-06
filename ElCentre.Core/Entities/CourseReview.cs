using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class CourseReview
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string ReviewContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int CourseId { get; set; }
        public Courses Course { get; set; }
    }
}
