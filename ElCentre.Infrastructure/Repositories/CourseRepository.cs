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

            var course = new Course
            {
                Title = addCourseDTO.Title,
                Description = addCourseDTO.Description,
                Price = addCourseDTO.Price,
                IsActive = addCourseDTO.IsActive,
                DurationInHours = addCourseDTO.DurationInHours,
                InstructorId = InstructorId,
                CategoryId = addCourseDTO.CategoryId,
                CourseStatus = "Pending",
            };

            if (addCourseDTO.Thumbnail != null)
            {
                if (courseThumbnailService == null) return false;
                var thumbnail = await courseThumbnailService.AddImageAsync(addCourseDTO.Thumbnail, addCourseDTO.Title);
                course.Thumbnail = thumbnail;
            }
            else
            {
                course.Thumbnail = "https://drive.google.com/uc?export=view&id=1T27W79Al7X4MFaZPwLQF7dJaC-9E39dY";
            }

            await context.Courses.AddAsync(course);
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

            courseThumbnailService.DeleteImageAsync(course.Thumbnail);
            context.Courses.Remove(course);
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
                 .Where(c => c.InstructorId == InstructorId && c.CourseStatus == "Approved")
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
                .Where(c => c.CourseStatus == "Approved" && c.IsActive)
                .AsNoTracking();

            // Search
            if (!string.IsNullOrEmpty(courseParams.search))
            {
                var searchword = courseParams.search.Split(' ');
                query = query.Where(m => searchword.All(
                word => m.Title.ToLower().Contains(word.ToLower())
                || //or
                m.Description.ToLower().Contains(word.ToLower())
                ));
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
            mapper.Map(updateCourseDTO, course);
            if (updateCourseDTO.Thumbnail != null)
            {
                courseThumbnailService.DeleteImageAsync(course.Thumbnail);
                var thumbnail = await courseThumbnailService.AddImageAsync(updateCourseDTO.Thumbnail, updateCourseDTO.Title);
                course.Thumbnail = thumbnail;
            }
            else
            {
                course.Thumbnail = course.Thumbnail;
            }
            await context.SaveChangesAsync();
            return true;
        }
    }
}
