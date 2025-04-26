using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElCentre.API.Controllers
{
    public class CourseController : BaseController
    {
        public CourseController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
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
                var result = await work.CourseRepository.AddAsync(addCourseDTO,InstructorId);
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
                if (updateCourseDTO == null)
                {
                    return BadRequest(new APIResponse(400, "Please Fill All Fields"));
                }
                    await work.CourseRepository.UpdateAsync(updateCourseDTO);
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
                var course = await work.CourseRepository
                    .GetByIdAsync(id, x => x.Category);
                await work.CourseRepository.DeleteAsync(course);
                return Ok(new APIResponse(200, "Course deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(400, ex.Message));
            }
        }

    }
}
