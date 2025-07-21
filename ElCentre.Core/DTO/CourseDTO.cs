using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ElCentre.Core.DTO
{
    public record AddCourseDTO
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string? Requirements { get; set; }

        public decimal Price { get; set; }

        public IFormFileCollection Thumbnail { get; set; }

        public bool IsActive { get; set; }

        public int DurationInHours { get; set; }

        public int CategoryId { get; set; }

        public bool UseAIAssistant { get; set; } = true;
    }
    public record CourseDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string? Requirements { get; set; }

        public decimal Price { get; set; }

        public string Thumbnail { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        public int DurationInHours { get; set; }

        public double Rating { get; set; }

        public string InstructorId { get; set; }

        public string InstructorName { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CourseStatus { get; set; }

        public string InstructorImage { get; set; }

        public bool IsDeleted { get; set; }

        public bool UseAIAssistant { get; set; } = true;
    }
   
    public record UpdateCourseDTO 
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string? Requirements { get; set; }

        public decimal Price { get; set; }

        public IFormFileCollection? Thumbnail { get; set; }

        public bool IsActive { get; set; }

        public int DurationInHours { get; set; }

        public int CategoryId { get; set; }

        public bool UseAIAssistant { get; set; } = true;
    }
}
