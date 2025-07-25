using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public class CompletedLessonsDTO
    {
        public int LessonId { get; set; }

        public int EnrollmentId { get; set; }

        public DateTime CompletedDate { get; set; }

    }
}
