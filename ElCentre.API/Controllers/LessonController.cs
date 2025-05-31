﻿using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ElCentre.API.Controllers
{
    public class LessonController : BaseController
    {
        public LessonController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
        }

        /// <summary>
        /// Get all lessons for a specific module
        /// </summary>
        /// <param name="moduleId">ID of the module</param>
        /// <returns>List of lessons</returns>
        [AllowAnonymous]
        [HttpGet("get-module-lessons")]
        public async Task<IActionResult> GetModuleLessons(int moduleId)
        {
            try
            {
                var lessons = await work.LessonRepository.GetLessonsByModuleIdAsync(moduleId);

                var lessonDtos = mapper.Map<IEnumerable<LessonDTO>>(lessons);
                return Ok(lessonDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(500, ex.Message));
            }
        }

        /// <summary>
        /// Get a specific lesson by ID
        /// </summary>
        /// <param name="id">Lesson ID</param>
        /// <returns>Lesson details</returns>
        [AllowAnonymous]
        [HttpGet("get-lesson-by-id/{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            try
            {
                var lesson = await work.LessonRepository.GetByIdAsync(id);

                if (lesson == null)
                {
                    return NotFound(new APIResponse(404, "Lesson not found."));
                }

                var lessonDto = mapper.Map<LessonDTO>(lesson);

                return Ok(lessonDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(500, ex.Message));
            }
        }

        /// <summary>
        /// Add a new lesson
        /// </summary>
        /// <param name="addLessonDto">Lesson data</param>
        /// <returns>Response indicating success or failure</returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost("add-lesson")]
        public async Task<IActionResult> AddLesson([FromForm] AddLessonDTO addLessonDto)
        {
            try
            {
                var InstructorId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (InstructorId == null)
                {
                    return BadRequest(new APIResponse(400, "Sign in first as instructor "));
                }
                var lesson = mapper.Map<Lesson>(addLessonDto);

                // Content is handled in the repository
                var result = await work.LessonRepository.AddWithOrderIndexAsync(lesson, addLessonDto.Content,InstructorId);

                if (result == null)
                {
                    return BadRequest(new APIResponse(400, "Failed to add lesson, you may don't have permission to add it"));
                }

                return Ok(new APIResponse(200, "Lesson added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(500, ex.Message));
            }
        }

        /// <summary>
        /// Update an existing lesson
        /// </summary>
        /// <param name="updateLessonDto">Updated lesson data</param>
        /// <returns>Response indicating success or failure</returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPut("update-lesson")]
        public async Task<IActionResult> UpdateLesson([FromForm] UpdateLessonDTO updateLessonDto)
        {
            try
            {
                var InstructorId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (InstructorId == null)
                {
                    return BadRequest(new APIResponse(400, "Sign in first as instructor "));
                }
                var lesson = mapper.Map<Lesson>(updateLessonDto);

                // Get the existing lesson to preserve ModuleId
                var existingLesson = await work.LessonRepository.GetByIdAsync(updateLessonDto.Id);
                if (existingLesson == null)
                {
                    return NotFound(new APIResponse(404, "Lesson not found."));
                }

                // Preserve the ModuleId from the database
                lesson.ModuleId = existingLesson.ModuleId;

                var result = await work.LessonRepository.UpdateLessonAsync(lesson, updateLessonDto.Content, InstructorId);

                if (result)
                {
                    return Ok(new APIResponse(200, "Lesson updated successfully"));
                }

                return BadRequest(new APIResponse(400, "Lesson not found or you don't have permission to update it."));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(500, ex.Message));
            }
        }

        /// <summary>
        /// Delete a lesson
        /// </summary>
        /// <param name="id">ID of the lesson to delete</param>
        /// <returns>Response indicating success or failure</returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpDelete("delete-lesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            try
            {
                var InstructorId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (InstructorId == null)
                {
                    return BadRequest(new APIResponse(400, "Sign in first as instructor "));
                }
               var result = await work.LessonRepository.DeleteAndReorderAsync(id,InstructorId);
                if (!result)
                {
                    return NotFound(new APIResponse(404, "Lesson not found or you don't have permission to delete it"));
                }
                return Ok(new APIResponse(200, "Lesson deleted successfully and lesson order updated"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(500, ex.Message));
            }
        }
    }
}
