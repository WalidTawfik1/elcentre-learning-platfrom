using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElCentre.API.Controllers.v2
{

    public class CourseReviewController : BaseController
    {
        public CourseReviewController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
        }

        /// <summary>
        /// Student adds a review for a course, Rating should be from 1 to 5.
        /// </summary>
        /// <param name="reviewDTO"></param>
        [Authorize(Roles = "Student")]
        [HttpPost("add-course-review")]
        public async Task<IActionResult> AddRating([FromBody] CourseReviewDTO reviewDTO)
        {
            if (reviewDTO == null)
            {
                return BadRequest(new APIResponse(400, "Invalid review data."));
            }

            try
            {
                var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(studentId))
                {
                    return Unauthorized(new APIResponse(401, "Please login or register."));
                }

                var result = await work.CourseReviewRepository.AddRatingAsync(reviewDTO, studentId);

                if (result)
                {
                    return Ok(new APIResponse(200, "Review added successfully."));
                }

                return BadRequest(new APIResponse(400, "Failed to add review. Please try again, or user already added a review before."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred,{ex.Message}."));
            }
        }


        /// <summary>
        /// Get all reviews for a course.
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns>A list of course reviews.</returns>
        [AllowAnonymous]
        [HttpGet("get-course-review/{courseId}")]
        public async Task<IActionResult> GetAllRatingForcourse(int courseId)
        {
            var result = await work.CourseReviewRepository.GetAllRatingForCourseAsync(courseId);
            return Ok(result);
        }
        /// <summary>
        /// Update a review for a course.
        /// </summary>
        /// <param name="reviewDTO"></param>
        [Authorize(Roles = "Student")]
        [HttpPut("update-course-review")]
        public async Task<IActionResult> UpdateReview([FromBody] UpdateReviewDTO reviewDTO)
        {
            var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized(new APIResponse(401, "Please Login or Register"));
            }

            var result = await work.CourseReviewRepository.UpdateReviewAsync(reviewDTO, studentId);
            return result ? Ok(new APIResponse(200, "Review Updated Successfully"))
                          : NotFound(new APIResponse(404, "Review Not Found or Unauthorized"));
        }
        /// <summary>
        /// Delete a review for a course.
        /// </summary>
        /// <param name="reviewId"></param>
        [Authorize(Roles = "Student")]
        [HttpDelete("delete-course-review/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized(new APIResponse(401, "Please Login or Register"));
            }

            var result = await work.CourseReviewRepository.DeleteReviewAsync(reviewId, studentId);
            return result ? Ok(new APIResponse(200, "Review Deleted Successfully"))
                          : NotFound(new APIResponse(404, "Review Not Found or Unauthorized"));
        }
    }
}
