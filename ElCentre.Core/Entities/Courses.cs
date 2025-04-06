using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class Courses
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Thumbnail { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int DurationInHours { get; set; }
        public string InstractorId { get; set; }
        public AppUser Insrtractor { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<Enrollments> Enrollments { get; set; }
        public ICollection<CourseModule> Modules { get; set; }
        public ICollection<CourseReview> Reviews { get; set; }
    }
}
