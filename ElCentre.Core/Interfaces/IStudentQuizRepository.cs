using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface IStudentQuizRepository : IGenericRepository<StudentQuiz>
    {
        Task<string> GetTotalQuizScoreAsync(string studentId, int lessonId);
        Task<StudentQuiz> SubmitQuizAnswerAsync(string studentId, int quizId, string answer);
        Task<IEnumerable<StudentQuiz>> GetStudentQuizzesByLessonAsync(string studentId, int lessonId);
    }
}
