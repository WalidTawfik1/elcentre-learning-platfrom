using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class HelpfulQ_A
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
