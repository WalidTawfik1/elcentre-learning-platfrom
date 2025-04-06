using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class CourseReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Rating { get; set; }

        public string ReviewContent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("User")]
        public string UserId { get; set; }
        public AppUser User { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
