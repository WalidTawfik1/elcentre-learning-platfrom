using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class ReportQ_A
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
