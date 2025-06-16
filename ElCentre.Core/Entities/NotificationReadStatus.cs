using System;
using System.ComponentModel.DataAnnotations;

namespace ElCentre.Core.Entities
{
    public class NotificationReadStatus
    {
        [Key]
        public int Id { get; set; }
        public int NotificationId { get; set; }
        public string UserId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Navigation property
        public CourseNotification Notification { get; set; }
    }
}