using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.Interfaces;
using ElCentre.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElCentre.API.Controllers
{
    [Authorize]
    public class Q_AController : BaseController
    {
        public Q_AController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
        }

        [HttpGet("get-all-lesson-questions/{lessonId}")]
        public async Task<IActionResult> GetAllLessonQuestions(int lessonId)
        {
            try
            {
                var questions = await work.Q_ARepository.GetQuestionsByLessonIdAsync(lessonId);

                    return Ok(questions);               
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        [HttpGet("get-all-question-answers/{questionId}")]
        public async Task<IActionResult> GetAllQuestionAnswers(int questionId)
        {
            try
            {
                var answers = await work.Q_ARepository.GetAnswersByQuestionIdAsync(questionId);

                return Ok(answers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        [HttpPost("add-question")]
        public async Task<IActionResult> AddQuestion(string question, int lessonId)
        {
            if (question == null)
            {
                return BadRequest(new APIResponse(400, "Invalid question data."));
            }
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = HttpContext.User.FindFirst(ClaimTypes.GivenName)?.Value;
                var userImage = HttpContext.User.FindFirst("ProfilePicture")?.Value;
                var isInstructor = HttpContext.User.IsInRole("Instructor");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new APIResponse(401, "Please login or register."));
                }
                var result = await work.Q_ARepository.AddQuestionAsync(question, userId, userName, userImage, isInstructor, lessonId);
                if (result)
                {
                    return Ok(new APIResponse(200, "Question added successfully."));
                }
                return BadRequest(new APIResponse(400, "Failed to add question. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        [HttpPost("add-answer")]
        public async Task<IActionResult> AddAnswer(string answer, int questionId)
        {
            if (answer == null)
            {
                return BadRequest(new APIResponse(400, "Invalid answer data."));
            }
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = HttpContext.User.FindFirst(ClaimTypes.GivenName)?.Value;
                var userImage = HttpContext.User.FindFirst("ProfilePicture")?.Value;
                var isInstructor = HttpContext.User.IsInRole("Instructor");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new APIResponse(401, "Please login or register."));
                }
                var result = await work.Q_ARepository.AddAnswerAsync(answer, userId, userName, userImage, isInstructor, questionId);
                if (result)
                {
                    return Ok(new APIResponse(200, "Answer added successfully."));
                }
                return BadRequest(new APIResponse(400, "Failed to add answer. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        [HttpDelete("delete-question/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new APIResponse(401, "Please login or register."));
                }

                var result = await work.Q_ARepository.DeleteQuestionAsync(questionId, userId);
                if (result)
                {
                    return Ok(new APIResponse(200, "Question deleted successfully."));
                }
                return BadRequest(new APIResponse(400, "Failed to delete question. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        [HttpDelete("delete-answer/{answerId}")]
        public async Task<IActionResult> DeleteAnswer(int answerId)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new APIResponse(401, "Please login or register."));
                }

                var result = await work.Q_ARepository.DeleteAnswerAsync(answerId, userId);
                if (result)
                {
                    return Ok(new APIResponse(200, "Answer deleted successfully."));
                }
                return BadRequest(new APIResponse(400, "Failed to delete answer. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        [HttpPut("update-question/{questionId}")]
        public async Task<IActionResult> UpdateQuestion(int questionId, string question)
        {
            if (string.IsNullOrEmpty(question))
            {
                return BadRequest(new APIResponse(400, "Invalid question data."));
            }
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new APIResponse(401, "Please login or register."));
                }
                var result = await work.Q_ARepository.UpdateQuestionAsync(questionId, userId, question);
                if (result)
                {
                    return Ok(new APIResponse(200, "Question updated successfully."));
                }
                return BadRequest(new APIResponse(400, "Failed to update question. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        [HttpPut("update-answer/{answerId}")]
        public async Task<IActionResult> UpdateAnswer(int answerId, string answer)
        {
            if (string.IsNullOrEmpty(answer))
            {
                return BadRequest(new APIResponse(400, "Invalid answer data."));
            }
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new APIResponse(401, "Please login or register."));
                }
                var result = await work.Q_ARepository.UpdateAnswerAsync(answerId, userId, answer);
                if (result)
                {
                    return Ok(new APIResponse(200, "Answer updated successfully."));
                }
                return BadRequest(new APIResponse(400, "Failed to update answer. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("pin-question/{questionId}")]
        public async Task<IActionResult> PinQuestion(int questionId, bool isPinned)
        {
            try
            {
                var result = await work.Q_ARepository.PinQuestionAsync(questionId, isPinned);
                if (result)
                {
                    return Ok(new APIResponse(200, "Question pin status updated successfully."));
                }
                return BadRequest(new APIResponse(400, "Failed to update question pin status. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }
    }
}