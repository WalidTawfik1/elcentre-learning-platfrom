using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class AppUser:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Courses> CreatedCourses { get; set; }
        public ICollection<Enrollments> Enrollments { get; set; } 
        public ICollection<Payment> payments { get; set; }
        public ICollection<CourseReview> CourseReviews { get; set; }
    }
}
