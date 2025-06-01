using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public class PendingCourseUpdateDto
    {
        public string Decision { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }
}
