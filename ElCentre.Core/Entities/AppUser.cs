using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public string Gender { get; set; }

        public string UserType { get; set; }

        public DateOnly DateOfBirth { get; set; }

        public string? Bio { get; set; }

        public string? ProfilePicture { get; set; }

        public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public ICollection<CourseReview> CourseReviews { get; set; } = new List<CourseReview>();

        public ICollection<StudentQuiz> StudentQuizzes { get; set; } = new List<StudentQuiz>();

    }
}
