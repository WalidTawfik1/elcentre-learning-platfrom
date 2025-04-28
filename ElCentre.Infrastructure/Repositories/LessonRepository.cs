using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class LessonRepository : GenericRepository<Lesson>, ILessonRepository
    {
        private readonly ElCentreDbContext _context;
        private readonly IVideoService _videoService;

        public LessonRepository(ElCentreDbContext context, IVideoService videoService) : base(context)
        {
            _context = context;
            _videoService = videoService;
        }

        public async Task<Lesson> AddWithOrderIndexAsync(Lesson entity, IFormFile content, string instructorId)
        {
            // Check if the module exists and belongs to the instructor
            var module = await _context.CourseModules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == entity.ModuleId && m.Course.InstructorId == instructorId);
            if (module == null)
                return null;

            // Get the highest OrderIndex for the specified module
            var maxOrderIndex = await _context.Lessons
                .Where(l => l.ModuleId == entity.ModuleId)
                .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

            // Set the new lesson's OrderIndex
            entity.OrderIndex = maxOrderIndex + 1;

            // Handle content based on ContentType
            if (content != null)
            {
                if (entity.ContentType == "video")
                {
                    // Upload video and store URL
                    string videoUrl = await _videoService.UploadVideoAsync(content);
                    entity.Content = videoUrl;
                }
                else if (entity.ContentType == "text")
                {
                    // For text content, read the file and convert to string
                    using var reader = new StreamReader(content.OpenReadStream());
                    entity.Content = await reader.ReadToEndAsync();
                }
            }

            // Add the entity using the base implementation
            await base.AddAsync(entity);

            return entity;
        }

        public async Task<bool> DeleteAndReorderAsync(int id, string instructorId)
        {
            // Get the lesson to be deleted
            var lessonToDelete = await _context.Lessons
                .Where(l => l.Module.Course.InstructorId == instructorId)
                .FirstOrDefaultAsync(l =>l.Id == id);
            if (lessonToDelete == null)
                return false;

            int moduleId = lessonToDelete.ModuleId;
            int deletedOrderIndex = lessonToDelete.OrderIndex;

            // If it's a video, delete from Cloudinary
            if (lessonToDelete.ContentType == "video" && !string.IsNullOrEmpty(lessonToDelete.Content))
            {
                try
                {
                    Uri uri = new Uri(lessonToDelete.Content);
                    string path = uri.AbsolutePath;
                    string[] segments = path.Split('/');
                    string filename = segments[segments.Length - 1];
                    string publicId = Path.GetFileNameWithoutExtension(filename);

                    await _videoService.DeleteVideoAsync(publicId);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with deletion
                    Console.WriteLine($"Error deleting video: {ex.Message}");
                }
            }

            // Delete the lesson
            _context.Lessons.Remove(lessonToDelete);

            // Get all lessons in the same module with higher OrderIndex
            var lessonsToUpdate = await _context.Lessons
                .Where(l => l.ModuleId == moduleId && l.OrderIndex > deletedOrderIndex)
                .ToListAsync();

            // Decrement OrderIndex for each affected lesson
            foreach (var lesson in lessonsToUpdate)
            {
                lesson.OrderIndex--;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<Lesson>> GetLessonsByModuleIdAsync(int moduleId)
        {
            return await _context.Lessons
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();
        }

        public async Task<bool> UpdateLessonAsync(Lesson lesson, IFormFile content, string instructorId)
        {
            try
            {
                // Check if the module exists and belongs to the instructor
                var module = await _context.CourseModules
                    .Include(m => m.Course)
                    .FirstOrDefaultAsync(m => m.Id == lesson.ModuleId && m.Course.InstructorId == instructorId);

                if (module == null) 
                    return false;

                // Get the existing lesson
                var existingLesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == lesson.Id);
                if (existingLesson == null)
                    return false;

                // Update basic properties
                existingLesson.Title = lesson.Title;
                existingLesson.DurationInMinutes = lesson.DurationInMinutes;
                existingLesson.IsPublished = lesson.IsPublished;

                // Update content if new content is provided
                if (content != null)
                {
                    // If content type is changed or new content is provided
                    bool contentTypeChanged = lesson.ContentType != existingLesson.ContentType;

                    // Delete old video if it was video type
                    if ((contentTypeChanged || lesson.ContentType == "video") &&
                        existingLesson.ContentType == "video" &&
                        !string.IsNullOrEmpty(existingLesson.Content))
                    {
                        try
                        {
                            Uri uri = new Uri(existingLesson.Content);
                            string path = uri.AbsolutePath;
                            string[] segments = path.Split('/');
                            string filename = segments[segments.Length - 1];
                            string publicId = Path.GetFileNameWithoutExtension(filename);

                            await _videoService.DeleteVideoAsync(publicId);
                        }
                        catch (Exception ex)
                        {
                            // Log the error but continue with update
                            Console.WriteLine($"Error deleting video: {ex.Message}");
                        }
                    }

                    // Update content type
                    existingLesson.ContentType = lesson.ContentType;

                    // Handle new content based on type
                    if (lesson.ContentType == "video")
                    {
                        existingLesson.Content = await _videoService.UploadVideoAsync(content);
                    }
                    else if (lesson.ContentType == "text")
                    {
                        using var reader = new StreamReader(content.OpenReadStream());
                        existingLesson.Content = await reader.ReadToEndAsync();
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating lesson: {ex.Message}");
                return false;
            }
        }
    }
}
