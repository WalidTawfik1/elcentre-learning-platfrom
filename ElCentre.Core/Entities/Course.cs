using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string? Requirements { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0m;

        public string Thumbnail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public int DurationInHours { get; set; }

        public double Rating { get; set; }

        public string CourseStatus { get; set; } = "Pending";

        [ForeignKey("Instructor")]
        public string InstructorId { get; set; }
        public AppUser Instructor { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();

        public ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
