using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface IQuizRepository : IGenericRepository<Quiz>
    {
        Task<IEnumerable<QuizDTO>> GetAllCourseQuizzesAsync(int CourseId);
    }
}
