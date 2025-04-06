using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class CourseModule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public int OrderIndex { get; set; }

        public bool IsPublished { get; set; } = true;

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public Course Course { get; set; } 

        public ICollection<Lesson> Lessons { get; set; }
    }
}
