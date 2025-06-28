using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Core.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElCentre.API.Controllers
{
    public class CourseController : BaseController
    {
        private readonly INotificationService _notificationService;
        public CourseController(IUnitofWork work, IMapper mapper, INotificationService notificationService) : base(work, mapper)
        {
            _notificationService = notificationService;
        }
        /// <summary>
        /// Get all courses with pagination
        /// </summary>
        /// <param name="courseParams"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-all-courses")]
        public async Task<IActionResult> GetAllCourses([FromQuery] CourseParams courseParams)
        {
            try
            {
                var courses = await work.CourseRepository.GetAllAsync(courseParams);
                var totalcount = await work.CourseRepository.CountAsync();
                return Ok(new Pagination<CourseDTO>(courseParams.pagenum, courseParams.pagesize, totalcount, courses));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all instructor courses for instructor
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Instructor")]
        [HttpGet("get-all-instructor-courses")]
        public async Task<IActionResult> GetAllInstructorCourses()
        {
            try
            {
                var InstructorId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (InstructorId == null)
                {
                    return BadRequest(new APIResponse(400, "Sign in first as instructor "));
                }

                var courses = await work.CourseRepository.GetAllbyInstructorIdAsync(InstructorId);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all approved instructor courses for students
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-all-approved-instructor-courses/{instructorId}")]
        public async Task<IActionResult> GetAllApprovedInstructorCourses(string instructorId)
        {
            try
            {
                var courses = await work.CourseRepository.GetAllApprovedbyInstructorIdAsync(instructorId);
                if (courses == null || !courses.Any())
                {
                    return NotFound(new APIResponse(404, "No courses found for this instructor."));
                }
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get course by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-course/{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            try
            {
                var course = await work.CourseRepository.GetByIdAsync(id,
                    x => x.Category,
                    x => x.Instructor,
                    x => x.Reviews);
                var result = mapper.Map<CourseDTO>(course);
                if (course == null)
                {
                    return BadRequest(new APIResponse(400, "This Course Not Found"));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Add a new course
        /// </summary>
        /// <param name="addCourseDTO"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost("add-course")]
        public async Task<IActionResult> AddCourse([FromForm] AddCourseDTO addCourseDTO)
        {
            var InstructorId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                if (addCourseDTO == null)
                {
                    return BadRequest(new APIResponse(400, "Please Fill All Fields"));
                }
                var result = await work.CourseRepository.AddAsync(addCourseDTO, InstructorId);
                if (result)
                {
                    return Ok(new APIResponse(200, "Course Added Successfully"));
                }
                return BadRequest(new APIResponse(400, "Failed to Add Course"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Update an existing course
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateCourseDTO"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPut("update-course")]
        public async Task<IActionResult> UpdateCourse([FromForm] UpdateCourseDTO updateCourseDTO)
        {
            try
            {
                var InstructorId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (InstructorId == null)
                {
                    return BadRequest(new APIResponse(400, "Sign in first as instructor "));
                }
                if (updateCourseDTO == null)
                {
                    return BadRequest(new APIResponse(400, "Please Fill All Fields"));
                }
                var result = await work.CourseRepository.UpdateAsync(updateCourseDTO, InstructorId);
                if (!result)
                {
                    return BadRequest(new APIResponse(400, "Course not found or you don't have a permission to update this course"));
                }
                return Ok(new APIResponse(200, "Course updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Delete course by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpDelete("delete-course/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var InstructorId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (InstructorId == null)
                {
                    return BadRequest(new APIResponse(400, "Sign in first as instructor "));
                }
                var result = await work.CourseRepository.DeleteAsync(id, InstructorId);
                if (!result)
                {
                    return BadRequest(new APIResponse(400, "Course not found or you don't have a permission to delete this course"));
                }
                return Ok(new APIResponse(200, "Course deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Get all pending courses for admin approval
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("get-pending-courses")]
        public async Task<IActionResult> GetPendingCourses()
        {
            try
            {
                var pendingCourses = await work.PendingCourseRepository.GetAllPendingCoursesAsync();
                return Ok(pendingCourses);
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Get a pending course by its ID for admin approval
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("get-pending-course/{courseId}")]
        public async Task<IActionResult> GetPendingCourseById(int courseId)
        {
            try
            {
                var pendingCourse = await work.PendingCourseRepository.GetPendingCourseByIdAsync(courseId);
                return Ok(pendingCourse);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new APIResponse(404, knfEx.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Update a pending course's status (approve or reject) by admin
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="decision">The decision (approve or reject)</param>
        /// <param name="rejectionReason"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("update-pending-course/{courseId}")]
        public async Task<IActionResult> UpdatePendingCourse(int courseId, [FromBody] PendingCourseUpdateDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Decision) ||
                    (request.Decision.ToLower() != "approve" && request.Decision.ToLower() != "reject"))
                {
                    return BadRequest(new APIResponse(400, "Decision must be either 'approve' or 'reject'."));
                }                var result = await work.PendingCourseRepository.UpdatePendingCourseAsync(
                    courseId, request.Decision, request.RejectionReason);

                var course = await work.CourseRepository.GetByIdAsync(courseId);
                
                // Send course status notification using the enhanced service
                var adminId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var adminName = HttpContext.User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Admin";
                
                if (!string.IsNullOrEmpty(adminId) && course != null)
                {
                    await _notificationService.NotifyCourseStatusChangeAsync(
                        courseId, 
                        request.Decision, 
                        course.InstructorId, 
                        adminId, 
                        adminName, 
                        request.RejectionReason);
                }

                if (!result)
                {
                    return BadRequest(new APIResponse(400, "Failed to update pending course."));
                }

                return Ok(new APIResponse(200, "Pending course updated successfully."));
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new APIResponse(404, knfEx.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(400, ex.Message));
            }
        }

    }
}