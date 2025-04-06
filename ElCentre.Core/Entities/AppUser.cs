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

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public ICollection<Course> CreatedCourses { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } 

        public ICollection<Payment> Payments { get; set; }

        public ICollection<CourseReview> CourseReviews { get; set; }
    }
}
