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
    public class QuizController : BaseController
    {
        public QuizController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
        }

        /// <summary>
        /// Get all quizzes for a course
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-all-course-quizzes")]
        public async Task<IActionResult> GetAllCourseQuizzes(int courseId)
        {
            try
            {
                var quizzes = await work.QuizRepository.GetAllCourseQuizzesAsync(courseId);
                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(500, ex.Message));
            }
        }

        /// <summary>
        /// Get quiz by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-quiz-by-id/{id}")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            try
            {
                var quiz = await work.QuizRepository.GetByIdAsync(id);
                if (quiz == null)
                {
                    return BadRequest(new APIResponse(400));
                }
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(500, ex.Message));
            }
        }

         /// <summary>
         /// Add Quiz to Course
         /// </summary>
         /// <param name="AddQuizDTO"></param>
         /// <returns></returns>
            [Authorize(Roles = "Instructor")]
            [HttpPost("add-quiz")]
            public async Task<IActionResult> AddQuiz([FromForm] AddQuizDTO AddQuizDTO)
            {
                try
                {
                    var quiz = mapper.Map<Quiz>(AddQuizDTO);
                    await work.QuizRepository.AddAsync(quiz);
                    return Ok(new APIResponse(200, "Quiz added successfully"));
                }
                catch (Exception ex)
                {
                    return BadRequest(new APIResponse(500, ex.Message));
                }
            }

         /// <summary>
         /// Update Quiz
         /// </summary>
         /// <param name="quizDTO"></param>
         /// <returns></returns>
             [Authorize(Roles = "Instructor")]
             [HttpPut("update-quiz")]
             public async Task<IActionResult> UpdateQuiz([FromForm] QuizDTO quizDTO)
             {
                 try
                 {
                     var quiz = mapper.Map<Quiz>(quizDTO);
                     await work.QuizRepository.UpdateAsync(quiz);
                     return Ok(new APIResponse(200, "Quiz updated successfully"));
                 }
                 catch (Exception ex)
                 {
                     return BadRequest(new APIResponse(500, ex.Message));

                 }

             }
        
        /// <summary>
        /// Delete quiz
        /// </summary>
        /// <param name="quizId"></param>
        /// <returns></returns>
            [Authorize(Roles = "Instructor")]
            [HttpDelete("delete-quiz/{quizId}")]
            public async Task<IActionResult> DeleteQuiz(int quizId)
            {
                try
                {
                    await work.QuizRepository.DeleteAsync(quizId);
                    return Ok(new APIResponse(200, "Quiz deleted successfully"));
                }
                catch (Exception ex)
                {
                    return BadRequest(new APIResponse(500, ex.Message));

                }

            }


    }
}

