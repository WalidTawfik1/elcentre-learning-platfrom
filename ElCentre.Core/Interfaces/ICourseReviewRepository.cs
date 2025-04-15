using ElCentre.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface ICourseReviewRepository
    {
        Task<bool> AddRatingAsync(CourseReviewDTO reviewDTO, string studentId);
        Task<IReadOnlyList<ReturnCourseReviewDTO>> GetAllRatingForCourseAsync(int CourseId);
        Task<bool> UpdateReviewAsync(UpdateReviewDTO updateReviewDTO, string studentId);
        Task<bool> DeleteReviewAsync(int reviewId, string studentId);
    }
}
