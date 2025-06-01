using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface IPendingCourseRepository
    {
        Task<CourseDTO> GetPendingCourseByIdAsync(int courseId);
        Task<IEnumerable<CourseDTO>> GetAllPendingCoursesAsync();
        Task<bool> UpdatePendingCourseAsync(int courseId, string decision,string? rejectionReason);
    }
}
