using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface ICourseRepository: IGenericRepository<Course>
    {
        Task<bool> AddAsync(AddCourseDTO addCourseDTO, string InstructorId);
        Task<bool> UpdateAsync(UpdateCourseDTO updateCourseDTO, string InstructorId);
        Task<bool> DeleteAsync(int courseId, string InstructorId);
        Task<IEnumerable<CourseDTO>> GetAllAsync(CourseParams courseParams);
        Task<IEnumerable<CourseDTO>> GetAllbyInstructorIdAsync(string InstructorId);
        Task<IEnumerable<CourseDTO>> GetAllApprovedbyInstructorIdAsync(string InstructorId);
        Task<bool> AdminDeleteAsync(int courseId, bool delete);
        Task<IEnumerable<CourseDTO>> GetAllForAdminAsync(CourseParams courseParams);

    }
}
