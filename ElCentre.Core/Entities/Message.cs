using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string SenderId { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsInstructor { get; set; } = false;

    }
}
