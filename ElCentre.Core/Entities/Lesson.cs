using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
        public int OrderIndex { get; set; }
        public int DurationTnMinutes { get; set; }
        public bool IsPublished { get; set; }
        public int ModuleId { get; set; }
        public CourseModule Module { get; set; }
    }
}
