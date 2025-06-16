using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class CourseNotification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int CourseId { get; set; }
        public string CreatedById { get; set; } // Instructor ID
        public string CreatedByName { get; set; } // Instructor Name
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string NotificationType { get; set; } // "NewLesson", "Announcement"
    }
}
