using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Core.Sharing;
using ElCentre.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly ElCentreDbContext context;
        private readonly IMapper mapper;
        private readonly ICourseThumbnailService courseThumbnailService;

        public CourseRepository(ICourseThumbnailService courseThumbnailService, IMapper mapper, ElCentreDbContext context) : base(context)
        {
            this.courseThumbnailService = courseThumbnailService;
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<bool> AddAsync(AddCourseDTO addCourseDTO, string InstructorId)
        {
            if (addCourseDTO == null) return false;

            // First get the instructor and category to satisfy required properties
            var instructor = await context.Users.FindAsync(InstructorId);
            var category = await context.Categories.FindAsync(addCourseDTO.CategoryId);
            
            if (instructor == null || category == null) return false;

            var course = new Course
            {
                Title = addCourseDTO.Title,
                Description = addCourseDTO.Description,
                Requirements = addCourseDTO.Requirements,
                Price = addCourseDTO.Price,
                IsActive = addCourseDTO.IsActive,
                DurationInHours = addCourseDTO.DurationInHours,
                InstructorId = InstructorId,
                Instructor = instructor,
                CategoryId = addCourseDTO.CategoryId,
                Category = category,
                CourseStatus = "Pending",
                UseAIAssistant = addCourseDTO.UseAIAssistant
            };

            if (addCourseDTO.Thumbnail != null)
            {
                if (courseThumbnailService == null) return false;
                var thumbnail = await courseThumbnailService.AddImageAsync(addCourseDTO.Thumbnail, addCourseDTO.Title);
                course.Thumbnail = thumbnail ?? "https://i.ibb.co/V0D34Xty/istockphoto-1147544806-170667a.jpg";
            }
            else
            {
                course.Thumbnail = "https://i.ibb.co/V0D34Xty/istockphoto-1147544806-170667a.jpg";
            }

            await context.Courses.AddAsync(course);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AdminDeleteAsync(int courseId, bool delete)
        {
            var course = await context.Courses.FindAsync(courseId);
            if (course == null) return false;
            course.IsDeleted = delete; // Soft delete
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int courseId, string InstructorId)
        {
            var course = await context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Where(c => c.InstructorId == InstructorId)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return false;

            if (course.Thumbnail != null)
            {
                courseThumbnailService.DeleteImageAsync(course.Thumbnail);
            }
            course.IsDeleted = true; // Soft delete
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CourseDTO>> GetAllApprovedbyInstructorIdAsync(string InstructorId)
        {
            var courses = context.Courses
                 .Include(c => c.Category)
                 .Include(c => c.Instructor)
                 .Include(c => c.Modules)
                 .Include(c => c.Reviews).ThenInclude(r => r.User)
                 .Where(c => c.InstructorId == InstructorId && c.CourseStatus == "Approved" && !c.IsDeleted)
                 .AsNoTracking();
            var result = mapper.Map<List<CourseDTO>>(courses);
            return result;
        }

        public async Task<IEnumerable<CourseDTO>> GetAllAsync(CourseParams courseParams)
        {
            var query = context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Include(c => c.Reviews).ThenInclude(r => r.User)
                .Where(c => c.CourseStatus == "Approved" && c.IsActive && !c.IsDeleted)
                .AsNoTracking();

            //filter by search
            if (!string.IsNullOrEmpty(courseParams.search))
            {
                var rawSearchWords = courseParams.search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Replace ه with ة only when it's the last character of each word
                var normalizedSearchWords = rawSearchWords.Select(word =>
                    word.EndsWith('ه') ? word.Substring(0, word.Length - 1) + 'ة' : word
                ).ToArray();

                if (normalizedSearchWords.Any())
                {
                    // Match ANY of the search words (name/description)
                    query = query.Where(m =>
                        normalizedSearchWords.Any(word =>
                            m.Title.ToLower().Contains(word.ToLower()) ||
                            m.Description.ToLower().Contains(word.ToLower()) ||
                            m.Category.Name.ToLower().Contains(word.ToLower())
                        )
                    );
                }
            }

            // Filter by category
            if (courseParams.categoryId.HasValue)
                query = query.Where(m => m.CategoryId == courseParams.categoryId);

            // Filter by price
            if (courseParams.minPrice != null && courseParams.maxPrice != null)
            {
                query = query.Where(m => m.Price >= courseParams.minPrice && m.Price <= courseParams.maxPrice);
            }
            else if (courseParams.minPrice != null)
            {
                query = query.Where(m => m.Price >= courseParams.minPrice);
            }
            else if (courseParams.maxPrice != null)
            {
                query = query.Where(m => m.Price <= courseParams.maxPrice);
            }

            if (!string.IsNullOrEmpty(courseParams.sort))
            {
                query = courseParams.sort switch
                {
                    "PriceAsc" => query.OrderBy(m => m.Price),
                    "PriceDesc" => query.OrderByDescending(m => m.Price),
                    "Rating" => query.OrderByDescending(m => m.Rating),
                    _ => query.OrderBy(m => m.Title),
                };
            }
            if (courseParams.sort == null) query = query.OrderBy(m => m.Title);

            query = query.Skip((courseParams.pagenum - 1) * courseParams.pagesize).Take(courseParams.pagesize);

            var result = mapper.Map<List<CourseDTO>>(query);
            return result;

        }

        public async Task<IEnumerable<CourseDTO>> GetAllbyInstructorIdAsync(string InstructorId)
        {
            var courses = context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Include(c => c.Reviews).ThenInclude(r => r.User)
                .Where(c => c.InstructorId == InstructorId)
                .AsNoTracking();
            var result = mapper.Map<List<CourseDTO>>(courses);
            return result;
        }

        public async Task<IEnumerable<CourseDTO>> GetAllForAdminAsync(CourseParams courseParams)
        {
            var query = context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Include(c => c.Reviews).ThenInclude(r => r.User)
                .AsQueryable();

            //filter by search
            if (!string.IsNullOrEmpty(courseParams.search))
            {
                var rawSearchWords = courseParams.search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Replace ه with ة only when it's the last character of each word
                var normalizedSearchWords = rawSearchWords.Select(word =>
                    word.EndsWith('ه') ? word.Substring(0, word.Length - 1) + 'ة' : word
                ).ToArray();

                if (normalizedSearchWords.Any())
                {
                    // Match ANY of the search words (name/description)
                    query = query.Where(m =>
                        normalizedSearchWords.Any(word =>
                            m.Title.ToLower().Contains(word.ToLower()) ||
                            m.Description.ToLower().Contains(word.ToLower()) ||
                            m.Category.Name.ToLower().Contains(word.ToLower())
                        )
                    );
                }
            }
            // Filter by category
            if (courseParams.categoryId.HasValue)
                query = query.Where(m => m.CategoryId == courseParams.categoryId);
            // Filter by price
            if (courseParams.minPrice != null && courseParams.maxPrice != null)
            {
                query = query.Where(m => m.Price >= courseParams.minPrice && m.Price <= courseParams.maxPrice);
            }
            else if (courseParams.minPrice != null)
            {
                query = query.Where(m => m.Price >= courseParams.minPrice);
            }
            else if (courseParams.maxPrice != null)
            {
                query = query.Where(m => m.Price <= courseParams.maxPrice);
            }
            if (!string.IsNullOrEmpty(courseParams.sort))
            {
                query = courseParams.sort switch
                {
                    "PriceAsc" => query.OrderBy(m => m.Price),
                    "PriceDesc" => query.OrderByDescending(m => m.Price),
                    "Rating" => query.OrderByDescending(m => m.Rating),
                    _ => query.OrderByDescending(m => m.CreatedAt),
                };
            }
            if (courseParams.sort == null) query = query.OrderByDescending(m => m.CreatedAt);
            query = query.Skip((courseParams.pagenum - 1) * courseParams.pagesize).Take(courseParams.pagesize);
            var result = mapper.Map<List<CourseDTO>>(query);
            return result;

        }

        public async Task<bool> UpdateAsync(UpdateCourseDTO updateCourseDTO, string InstructorId)
        {
            if(updateCourseDTO == null) return false;
            var course = await context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Where(c => c.InstructorId == InstructorId)
                .FirstOrDefaultAsync(c => c.Id == updateCourseDTO.Id);
            if (course == null) return false;
            
            // Store the current thumbnail before mapping
            var currentThumbnail = course.Thumbnail;
            
            // Map the DTO to the entity (excluding Thumbnail which is ignored in mapping)
            mapper.Map(updateCourseDTO, course);
            
            // Handle thumbnail separately
            if (updateCourseDTO.Thumbnail != null)
            {
                // Delete the old thumbnail
                if (currentThumbnail != null)
                {
                    courseThumbnailService.DeleteImageAsync(currentThumbnail);
                }
                var thumbnail = await courseThumbnailService.AddImageAsync(updateCourseDTO.Thumbnail, updateCourseDTO.Title);
                course.Thumbnail = thumbnail; // thumbnail can be null if upload fails
            }
            else
            {
                // If no new thumbnail provided, keep the current one (can be null)
                course.Thumbnail = currentThumbnail;
            }

            if(course.CourseStatus == "Rejected")
            {
                course.CourseStatus = "Pending";
            }
            await context.SaveChangesAsync();
            return true;
        }
    }
}
