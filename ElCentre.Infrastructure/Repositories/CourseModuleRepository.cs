using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class CourseModuleRepository : GenericRepository<CourseModule>, ICourseModuleRepository
    {
        private readonly ElCentreDbContext _context;
        private readonly IMapper _mapper;

        public CourseModuleRepository(ElCentreDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<CourseModule> AddWithOrderIndexAsync(CourseModule entity, string instructorId)
        {
            // Check if the course exists and belongs to the instructor
            var validCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == entity.CourseId && c.InstructorId == instructorId);

            if (validCourse == null)
                return null;

            // Get the highest OrderIndex for the specified course
            var maxOrderIndex = await _context.CourseModules
                .Where(m => m.CourseId == entity.CourseId)
                .MaxAsync(m => (int?)m.OrderIndex) ?? 0;

            // Set the new module's OrderIndex to one more than the highest existing index
            entity.OrderIndex = maxOrderIndex + 1;

            // Add the entity using the base implementation
            await base.AddAsync(entity);

            return entity;
        }

        public async Task<bool> DeleteAndReorderAsync(int id, string instructorId)
        {
            // Get the module to be deleted
            var moduleToDelete = await _context.CourseModules
                .Where(m => m.Course.InstructorId == instructorId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (moduleToDelete == null)
                return false; // Module not found or instructor doesn't own it

            int courseId = moduleToDelete.CourseId;
            int deletedOrderIndex = moduleToDelete.OrderIndex;

            // Delete the module
            _context.CourseModules.Remove(moduleToDelete);

            // Get all modules in the same course with higher OrderIndex
            var modulesToUpdate = await _context.CourseModules
                .Where(m => m.CourseId == courseId && m.OrderIndex > deletedOrderIndex)
                .ToListAsync();

            // Decrement OrderIndex for each affected module
            foreach (var module in modulesToUpdate)
            {
                module.OrderIndex--;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            return true; // Deletion successful
        }

        public async Task<IReadOnlyList<CourseModule>> GetModulesByCourseIdAsync(int courseId)
        {
            return await _context.CourseModules
                .Where(m => m.CourseId == courseId)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();
        }

        public async Task<bool> UpdateCourseModuleAsync(CourseModule courseModule, string instructorId)
        {
            try
            {
                // Get the existing module with its current CourseId
                var existingModule = await _context.CourseModules
                    .Where(m => m.Course.InstructorId == instructorId)
                    .FirstOrDefaultAsync(m => m.Id == courseModule.Id);
                if (existingModule == null)
                    return false;

                // Update only the fields that should be updated
                existingModule.Title = courseModule.Title;
                existingModule.Description = courseModule.Description;
                existingModule.IsPublished = courseModule.IsPublished;

                // CourseId remains unchanged

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
