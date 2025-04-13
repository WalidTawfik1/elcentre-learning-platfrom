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
        Task<Lesson> AddWithOrderIndexAsync(Lesson entity, IFormFile content);
        Task<bool> UpdateLessonAsync(Lesson lesson, IFormFile content);
        Task DeleteAndReorderAsync(int id);
        Task<IReadOnlyList<Lesson>> GetLessonsByModuleIdAsync(int moduleId);
    }
}
