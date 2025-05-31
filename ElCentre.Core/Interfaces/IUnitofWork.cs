using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface IUnitofWork
    {
        IAuthentication Authentication { get; }
        ICategoryRepository CategoryRepository { get; }
        IUserRepository UserRepository { get; }
        ICourseRepository CourseRepository { get; }
        ICourseModuleRepository CourseModuleRepository { get; }
        ILessonRepository LessonRepository { get; }
        IEnrollmentRepository EnrollmentRepository { get; }
        ICourseReviewRepository CourseReviewRepository { get; }
        IQuizRepository QuizRepository { get; }
        IStudentQuizRepository StudentQuizRepository { get; }
    }
}
