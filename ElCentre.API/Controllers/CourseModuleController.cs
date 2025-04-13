using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElCentre.API.Controllers
{

    public class CourseModuleController : BaseController
    {
        public CourseModuleController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
        }

        /// <summary>
        /// Get All Course Modules
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-all-course-modules")]
        public async Task<IActionResult> GetAllCourseModules(int courseId)
        {
            try
            {
                var courseModules = await work.CourseModuleRepository.GetModulesByCourseIdAsync(courseId);

                if (courseModules == null || !courseModules.Any())
                {
                    return NotFound(new APIResponse(404, "No course modules found."));
                }

                return Ok(courseModules);
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(500, ex.Message));
            }
        }


        /// <summary>
        /// Get Course Module By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-course-module-by-id/{id}")]
        public async Task<IActionResult> GetCourseModuleById(int id, int courseId)
        {
            try
            {
                var courseModule = await work.CourseModuleRepository.GetByIdAsync(id);
                if (courseModule == null || courseModule.CourseId != courseId)
                {
                    return NotFound(new APIResponse(404, "No course module found."));
                }

                return Ok(courseModule);
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(500, ex.Message));
            }
        }

        /// <summary>
        /// Add Course Module
        /// </summary>
        /// <param name="addCourseModule"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost("add-course-module")]
        public async Task<IActionResult> AddCourseModule([FromForm] AddCourseModuleDTO addCourseModule)
        {
            try
            {
                var courseModule = mapper.Map<CourseModule>(addCourseModule);
                await work.CourseModuleRepository.AddWithOrderIndexAsync(courseModule);
                return Ok(new APIResponse(200, "Course module added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update Course Module
        /// </summary>
        /// <param name="updateCourseModule"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPut("update-course-module")]
        public async Task<IActionResult> UpdateCourseModule([FromForm] UpdateCourseModuleDTO updateCourseModule)
        {
            try
            {
                var courseModule = mapper.Map<CourseModule>(updateCourseModule);
                var result = await work.CourseModuleRepository.UpdateCourseModuleAsync(courseModule);

                if (result)
                    return Ok(new APIResponse(200, "Course module updated successfully"));
                else
                    return NotFound(new APIResponse(404, "Course module not found or could not be updated"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete Course Module
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpDelete("delete-course-module/{id}")]
        public async Task<IActionResult> DeleteCourseModule(int id)
        {
            try
            {
                await work.CourseModuleRepository.DeleteAndReorderAsync(id);
                return Ok(new APIResponse(200, "Course module deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
