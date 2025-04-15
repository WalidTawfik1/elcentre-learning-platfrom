using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface ICourseModuleRepository : IGenericRepository<CourseModule>
    {
        Task<bool> UpdateCourseModuleAsync(CourseModule courseModule);
        Task<CourseModule> AddWithOrderIndexAsync(CourseModule courseModule);
        Task DeleteAndReorderAsync(int id);
        Task<IReadOnlyList<CourseModule>> GetModulesByCourseIdAsync(int courseId);
    }
}
