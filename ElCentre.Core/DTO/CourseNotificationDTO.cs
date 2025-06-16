using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public class CourseNotificationDTO
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public int CourseId { get; set; }
        public string NotificationType { get; set; } = "Custom";
    
    }
}
