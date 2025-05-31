using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class StudentQuiz
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public string StudentId { get; set; }
        public AppUser Student {  get; set; }

        [ForeignKey("Quiz")]
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
            
        [Required]
        public string Answer { get; set; }

        public int Score { get; set; }

        public DateTime TakenAt { get; set; } = DateTime.Now;

    }
}
