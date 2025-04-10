using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Core.Sharing;
using ElCentre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly ElCentreDbContext context;
        private readonly IMapper mapper;
        private readonly ICourseThumbnailService courseThumbnailService;

        public CourseRepository(ICourseThumbnailService courseThumbnailService, IMapper mapper, ElCentreDbContext context):base(context)
        {
            this.courseThumbnailService = courseThumbnailService;
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<bool> AddAsync(AddCourseDTO addCourseDTO)
        {
            if (addCourseDTO == null) return false;
            var course = mapper.Map<Course>(addCourseDTO);
            if (addCourseDTO.Thumbnail != null)
            {
                var thumbnail = await courseThumbnailService.AddImageAsync(addCourseDTO.Thumbnail,addCourseDTO.Title);
                course.Thumbnail = thumbnail;
            }
            else
            {
                course.Thumbnail = "default.png";
            }
            await context.Courses.AddAsync(course);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAsync(Course course)
        {
            courseThumbnailService.DeleteImageAsync(course.Thumbnail);
            context.Courses.Remove(course);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CourseDTO>> GetAllAsync(CourseParams courseParams)
        {
            var query = context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Include(c => c.Reviews).ThenInclude(r => r.User)
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
            if (courseParams.categoryId != 0)
            {
                query = query.Where(c => c.CategoryId == courseParams.categoryId);
            }

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

            var result = await query.ToListAsync();
            return mapper.Map<List<CourseDTO>>(result);
        }

        public async Task<bool> UpdateAsync(UpdateCourseDTO updateCourseDTO)
        {
            if(updateCourseDTO == null) return false;
            var course = await context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
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
