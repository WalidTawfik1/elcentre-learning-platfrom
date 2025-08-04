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

        /// <summary>
        /// Gets all questions related to a specific lesson.
        /// </summary>
        /// <param name="lessonId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets all answers related to a specific question.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds a new question to a lesson.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="lessonId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds a new answer to a specific question.
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Deletes a specific question.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
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

                var result = await work.Q_ARepository.DeleteQuestionAsync(questionId);
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

        /// <summary>
        /// Deletes a specific answer.
        /// </summary>
        /// <param name="answerId"></param>
        /// <returns></returns>
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

                var result = await work.Q_ARepository.DeleteAnswerAsync(answerId);
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

        /// <summary>
        /// Updates a specific question.
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="question"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates a specific answer.
        /// </summary>
        /// <param name="answerId"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Pins or unpins a specific question.
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="isPinned"></param>
        /// <returns></returns>
        [Authorize(Roles = "Instructor")]
        [HttpPut("pin-question/{questionId}")]
        public async Task<IActionResult> PinQuestion(int questionId, bool isPinned)
        {
            try
            {
                var result = await work.Q_ARepository.PinQuestionAsync(questionId, isPinned);
                if (result)
                {
                    if (isPinned)
                    {
                        return Ok(new APIResponse(200, "Question pinned successfully."));
                    }
                    return Ok(new APIResponse(200, "Question unpinned successfully."));
                }
                return BadRequest(new APIResponse(400, "Failed to update question pin status. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        /// <summary>
        /// Reports a question or answer.
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="answerId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        [HttpPost("report-qa")]
        public async Task<IActionResult> ReportQA(int? questionId, int? answerId, string reason)
        {
            if (questionId == null && answerId == null)
            {
                return BadRequest(new APIResponse(400, "Invalid request. Either questionId or answerId must be provided."));
            }
            if (string.IsNullOrEmpty(reason))
            {
                return BadRequest(new APIResponse(400, "Reason for reporting is required."));
            }
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new APIResponse(401, "Please login or register."));
                }
                var result = await work.Q_ARepository.ReportQA(questionId, answerId, userId, reason);
                if (result)
                {
                    return Ok(new APIResponse(200, "Report submitted successfully."));
                }
                return BadRequest(new APIResponse(400, "You've already reported this or something went wrong."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }

        /// <summary>
        /// Marks a question or answer as helpful.
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="answerId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("helpful-qa")]
        public async Task<IActionResult> HelpfulQA(int? questionId, int? answerId)
        {
            if (questionId == null && answerId == null)
            {
                return BadRequest(new APIResponse(400, "Invalid request. Either questionId or answerId must be provided."));
            }
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new APIResponse(401, "Please login or register first."));
            }
            try
            {

                var result = await work.Q_ARepository.HelpfulQA(questionId, answerId, userId);
                if (result)
                {
                    return Ok(new APIResponse(200, "Marked as helpful successfully."));
                }
                return BadRequest(new APIResponse(400, "You've already marked this as helpful or something went wrong."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse(500, $"An unexpected error occurred: {ex.Message}."));
            }
        }
    }
}