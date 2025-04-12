using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public record AddCourseModuleDTO
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsPublished { get; set; } = true;

        public int CourseId { get; set; }

    }

    public record UpdateCourseModuleDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsPublished { get; set; } = true;
    }
}
