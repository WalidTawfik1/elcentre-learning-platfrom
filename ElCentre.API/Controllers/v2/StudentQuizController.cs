using AutoMapper;
using ElCentre.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElCentre.API.Controllers.v2
{
    public class StudentQuizController : BaseController
    {
        public StudentQuizController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
        }

        /// <summary>
        /// Student submit quiz answer
        /// </summary>
        /// <param name="quizId"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        [Authorize(Roles = "Student")]
        [HttpPost("submit-quiz-answer")]
        public async Task<IActionResult> SubmitQuizAnswerAsync(int quizId, string answer)
        {
            var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(studentId == null)
            {
                return Unauthorized("You must be logged in to submit a quiz answer.");
            }
            var result = await work.StudentQuizRepository.SubmitQuizAnswerAsync(studentId,quizId,answer);
            if (result == null)
            {
                return Conflict("You have already submitted an answer for this quiz.");
            }
            return Ok(true);
        }

        /// <summary>
        /// Get total quiz score
        /// </summary>
        /// <param name="lessonId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Student")]
        [HttpGet("get-total-score")]
        public async Task<IActionResult> GetTotalQuizScoreAsync(int lessonId)
        {
            var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (studentId == null)
            {
                return Unauthorized("You must be logged in to get your total quiz score.");
            }
            var score = await work.StudentQuizRepository.GetTotalQuizScoreAsync(studentId, lessonId);
            return Ok(score);
        }

        /// <summary>
        /// Get answered student quizzes by lesson id
        /// </summary>
        /// <param name="lessonId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Student")]
        [HttpGet("get-student-quizzes-by-lesson")]
        public async Task<IActionResult> GetStudentQuizzesByLesson(int lessonId)
        {
            var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (studentId == null)
            {
                return Unauthorized("You must be logged in to get your total quiz score.");
            }
            var studentQuizzes = await work.StudentQuizRepository.GetStudentQuizzesByLessonAsync(studentId, lessonId);
            return Ok(studentQuizzes);
        }

    }
}
