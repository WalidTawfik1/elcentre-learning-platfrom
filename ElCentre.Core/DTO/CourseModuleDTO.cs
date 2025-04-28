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

    public record CourseModuleDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; } = true;
        public int OrderIndex { get; set; }
       // public CourseDto Course { get; set; }
    }
    //public class CourseDto
    //{
    //    public int Id { get; set; }
    //    public string Title { get; set; }
    //    // Include only necessary properties
    //    public UserDto Instructor { get; set; }
    //}

    //public class UserDto
    //{
    //    public string Id { get; set; }
    //    // No reference back to CreatedCourses
    //}

    // If wanted to include Lessons in the CourseModuleDTO, uncomment the following code
    /*public class CourseModuleDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public List<LessonsDTO> Lessons { get; set; }
    }

    public class LessonsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int OrderIndex { get; set; }
        public int DurationInMinutes { get; set; }

    }*/

}
