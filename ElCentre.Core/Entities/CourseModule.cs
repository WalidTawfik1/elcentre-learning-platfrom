using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class CourseModule
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        public bool IsPublished { get; set; }
        public int CourseId { get; set; }
        public Courses Course { get; set; } 
        public ICollection<Lesson> Lessons { get; set; }
    }
}
