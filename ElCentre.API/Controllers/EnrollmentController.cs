using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElCentre.API.Controllers
{
    public class EnrollmentController : BaseController
    {
        public EnrollmentController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
        }

        /// <summary>
        /// Enroll a student in a course
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollStudent(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Invalid course ID.");
            var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var result = await work.EnrollmentRepository.AddEnrollmentAsync(courseId, studentId);
            if (result != null)
                return Ok("Student enrolled successfully.");
            return BadRequest("Failed to enroll student.");
        }

        /// <summary>
        /// Check if a student is enrolled in a course
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("is-enrolled")]
        public async Task<IActionResult> IsEnrolled(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Invalid course ID.");
            var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var result = await work.EnrollmentRepository.IsStudentEnrolledInCourseAsync(studentId, courseId);
            return Ok(result);
        }

        /// <summary>
        /// Get enrollment by ID for Admin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Instructor")]
        [HttpGet("get-enrollment/{id}")]
        public async Task<IActionResult> GetEnrollment(int id)
        {
            try
            {
                var enrollment = await work.EnrollmentRepository.GetByIdAsync(id);
                if (enrollment == null)
                {
                    return BadRequest(new APIResponse(400, "This Enrollment Not Found"));
                }
                return Ok(enrollment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all course enrollments for instructor
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Instructor")]
        [HttpGet("get-course-enrollments")]
        public async Task<IActionResult> GetCourseEnrollments(int courseId)
        {
            try
            {
                var enrollments = await work.EnrollmentRepository.GetCourseEnrollmentsAsync(courseId);
                if (enrollments == null)
                {
                    return BadRequest(new APIResponse(400, "This Course Not Found"));
                }
                return Ok(enrollments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all enrollments for a student
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Student")]
        [HttpGet("get-student-enrollments")]
        public async Task<IActionResult> GetStudentEnrollments()
        {
            try
            {
                var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    return BadRequest(new APIResponse(400, "Student ID is required"));
                }
                var enrollments = await work.EnrollmentRepository.GetStudentEnrollments(studentId);
                if (enrollments == null || !enrollments.Any())
                {
                    return NoContent();
                }
                return Ok(enrollments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Mark a lesson as completed for the current student
        /// </summary>
        [Authorize]
        [HttpPost("complete-lesson/{lessonId}")]
        public async Task<IActionResult> MarkLessonAsCompleted(int lessonId)
        {
            try
            {
                if (lessonId <= 0)
                    return BadRequest(new APIResponse(400, "Invalid lesson ID"));

                string studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                var result = await work.EnrollmentRepository.MarkLessonAsCompletedAsync(lessonId, studentId);
                if (result)
                    return Ok(new APIResponse(200, "Lesson marked as completed"));

                return BadRequest(new APIResponse(400, "Failed to mark lesson as completed"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Check if a lesson is completed by the current student
        /// </summary>
        [Authorize]
        [HttpGet("is-lesson-completed/{lessonId}")]
        public async Task<IActionResult> IsLessonCompleted(int lessonId)
        {
            try
            {
                if (lessonId <= 0)
                    return BadRequest(new APIResponse(400, "Invalid lesson ID"));

                string studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                var result = await work.EnrollmentRepository.IsLessonCompletedAsync(lessonId, studentId);
                return Ok(new { isCompleted = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all completed lesson for a course
        /// </summary>
        [Authorize]
        [HttpGet("completed-lessons/{courseId}")]
        public async Task<IActionResult> GetCompletedLessons(int courseId)
        {
            try
            {
                if (courseId <= 0)
                    return BadRequest(new APIResponse(400, "Invalid course ID"));

                string studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                var completedLessonIds = await work.EnrollmentRepository.GetCompletedLessonIdsAsync(studentId, courseId);
                return Ok(completedLessonIds);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Recalculate and update progress for an enrollment
        /// </summary>
        [Authorize]
        [HttpPost("recalculate-progress/{enrollmentId}")]
        public async Task<IActionResult> RecalculateProgress(int enrollmentId)
        {
            try
            {
                if (enrollmentId <= 0)
                    return BadRequest(new APIResponse(400, "Invalid enrollment ID"));

                var progress = await work.EnrollmentRepository.CalculateAndUpdateProgressAsync(enrollmentId);
                return Ok(new { progress });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get the count of students enrolled in a course
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("students-count/{courseId}")]
        public async Task<IActionResult> GetStudentsCount(int courseId)
        {
            try
            {
                if (courseId <= 0)
                    return BadRequest(new APIResponse(400, "Invalid course ID"));
                var count = await work.EnrollmentRepository.GetStudentsCount(courseId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Un-enroll a student from a course
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("unenroll/{courseId}")]
        public async Task<IActionResult> UnEnroll(int courseId)
        {
            try
            {
                if (courseId <= 0)
                    return BadRequest(new APIResponse(400, "Invalid course ID"));
                var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var result = await work.EnrollmentRepository.UnEnroll(courseId, studentId);
                if (result)
                    return Ok(new APIResponse(200, "Unenrolled successfully."));
                
                return BadRequest(new APIResponse(400, "Failed to unenroll."));
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                
                // Include inner exception details if they exist
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner exception: {ex.InnerException.Message}";
                }
                
                return BadRequest(new APIResponse(400, errorMessage));
            }
        }

        /// <summary>
        /// Uncomplete a lesson for the student
        /// </summary>
        /// <param name="lessonId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("uncomplete-lesson/{lessonId}")]
        public async Task<IActionResult> UnCompleteLesson(int lessonId)
        {
            try
            {
                if (lessonId <= 0)
                    return BadRequest(new APIResponse(400, "Invalid lesson ID"));
                string studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var result = await work.EnrollmentRepository.UnCompleteAsync(lessonId, studentId);
                if (result)
                    return Ok(new APIResponse(200, "Lesson uncompleted successfully."));
                return BadRequest(new APIResponse(400, "Failed to uncomplete lesson."));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
