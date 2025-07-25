using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public record AddLessonDTO
    {
        public string Title { get; set; }

        public IFormFile Content { get; set; }

        public string ContentType { get; set; }

        public int? DurationInMinutes { get; set; }

        public string? Description { get; set; }

        public bool IsPublished { get; set; } = true;

        public int ModuleId { get; set; }
    }

    public record UpdateLessonDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int? DurationInMinutes { get; set; }

        public string? Description { get; set; }

        public bool IsPublished { get; set; } = true;

        public string? Content { get; set; }

    }

    public record LessonDTO
    {
        public int Id { get; set; }

        public int OrderIndex { get; set; }

        public int ModuleId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ContentType { get; set; }

        public int DurationInMinutes { get; set; }

        public string Description { get; set; }

        public bool IsPublished { get; set; } = true;

    }
}
