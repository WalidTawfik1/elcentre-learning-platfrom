using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface ILessonRepository : IGenericRepository<Lesson>
    {
        Task<Lesson> AddWithOrderIndexAsync(Lesson entity, IFormFile content, string instructorId);
        Task<bool> UpdateLessonAsync(Lesson lesson, string instructorId);
        Task<bool> DeleteAndReorderAsync(int id,string instructorId);
        Task<IReadOnlyList<Lesson>> GetLessonsByModuleIdAsync(int moduleId);
    }
}
