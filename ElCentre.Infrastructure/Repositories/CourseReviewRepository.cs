using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class CourseReviewRepository : ICourseReviewRepository
    {
        private readonly ElCentreDbContext _context;

        public CourseReviewRepository(ElCentreDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddRatingAsync(CourseReviewDTO reviewDTO, string studentId)
        {
            if (await _context.CourseReviews.AsNoTracking().AnyAsync(x => x.CourseId == reviewDTO.CourseId && x.UserId == studentId))
            {
                return false; // Review already exists
            }

            if(await _context.Enrollments.AsNoTracking().AnyAsync(x => x.StudentId == studentId && x.CourseId == reviewDTO.CourseId) == false)
            {
                return false; // Student is not enrolled in the course
            }

            var courseReview = new CourseReview
            {
                CourseId = reviewDTO.CourseId,
                UserId = studentId,
                Rating = reviewDTO.Rating,
                ReviewContent = reviewDTO.ReviewContent,
                CreatedAt = DateTime.Now
            };
            await _context.CourseReviews.AddAsync(courseReview);
            await _context.SaveChangesAsync();

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == reviewDTO.CourseId);
            var ratings = await _context.CourseReviews.AsNoTracking().Where(m => m.CourseId == course.Id).ToListAsync();

            if (ratings.Count > 0)
            {
                double? average = ratings.Average(m => m.Rating);
                double roundedReview = Math.Round((double)average, 1);
                course.Rating = (double)roundedReview;
            }
            else
            {
                course.Rating = (double)reviewDTO.Rating;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string studentId)
        {
            var review = await _context.CourseReviews.FirstOrDefaultAsync(x => x.Id == reviewId && x.UserId == studentId);
            if (review == null)
            {
                return false; // Review not found or does not belong to the user
            }
            _context.CourseReviews.Remove(review);
            await _context.SaveChangesAsync();
            var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == review.CourseId);
            var ratings = await _context.CourseReviews.AsNoTracking().Where(m => m.CourseId == course.Id).ToListAsync();
            if (ratings.Count > 0)
            {
                double? average = ratings.Average(m => m.Rating);
                double roundedReview = Math.Round((double)average, 1);
                course.Rating = (double)roundedReview;
            }
            else
            {
                course.Rating = 0;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<ReturnCourseReviewDTO>> GetAllRatingForCourseAsync(int CourseId)
        {
            var ratings = await _context.CourseReviews.Include(m => m.User).AsNoTracking().Where(m => m.CourseId == CourseId).ToListAsync();

            return ratings.Select(m => new ReturnCourseReviewDTO
            {
                StudentId = m.UserId,
                Id = m.Id,
                StudentName = m.User.FirstName + " " + m.User.LastName,
                Rating = (int)m.Rating,
                ReviewContent = m.ReviewContent,
                CreatedAt = m.CreatedAt,
                Count = ratings.Count

            }).ToList();
        }

        public async Task<bool> UpdateReviewAsync(UpdateReviewDTO updateReviewDTO, string studentId)
        {
            var review = await _context.CourseReviews.FirstOrDefaultAsync(m => m.Id == updateReviewDTO.Id && m.UserId == studentId);

            if (review == null)
            {
                return false;
            }

            review.Rating = updateReviewDTO.Rating;
            if (!string.IsNullOrWhiteSpace(updateReviewDTO.ReviewContent))
            {
                review.ReviewContent = updateReviewDTO.ReviewContent;
            }

            _context.CourseReviews.Update(review);
            await _context.SaveChangesAsync();

            // Recalculate course rating
            var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == review.CourseId);
            var ratings = await _context.CourseReviews.AsNoTracking().Where(m => m.CourseId == course.Id).ToListAsync();

            course.Rating = ratings.Any()
                 ? Math.Round((double)ratings.Average(m => m.Rating), 1)
                : Math.Round((double)updateReviewDTO.Rating, 1);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountReviewsForProductAsync(int courseId)
        {
            return await _context.CourseReviews.CountAsync(m => m.CourseId == courseId);
        }
    }
}
