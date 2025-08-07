using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface IEnrollmentRepository: IGenericRepository<Enrollment>
    {
        Task<Enrollment> AddEnrollmentAsync(int courseId, string studentId);
        Task<bool> IsStudentEnrolledInCourseAsync(string studentId, int courseId);
        Task<List<EnrollmentDTO>> GetStudentEnrollments(string studentId);
        Task<bool> MarkLessonAsCompletedAsync(int lessonId, string studentId);
        Task<bool> IsLessonCompletedAsync(int lessonId, string studentId);
        Task<List<CompletedLessonsDTO>> GetCompletedLessonIdsAsync(string studentId, int courseId);
        Task<float> CalculateAndUpdateProgressAsync(int enrollmentId);
        Task<List<EnrollmentDTO>> GetCourseEnrollmentsAsync(int courseId);
        Task<int> GetStudentsCount(int courseId);
        Task<bool> UnEnroll(int courseId, string studentId);
        Task<bool> UnCompleteAsync(int lessonId, string studentId);

    }
}
