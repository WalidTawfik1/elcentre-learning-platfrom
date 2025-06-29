using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface IQ_A
    {
        Task<bool> AddQuestionAsync(string question, string createdById, string createdByName, string creatorImage, bool isInstructor, int lessonId);
        Task<bool> AddAnswerAsync(string answer, string createdById, string createdByName, string creatorImage, bool isInstructor, int questionId);
        Task<IEnumerable<LessonQuestion>> GetQuestionsByLessonIdAsync(int lessonId);
        Task<IEnumerable<LessonAnswer>> GetAnswersByQuestionIdAsync(int questionId);
        Task<bool> DeleteQuestionAsync(int questionId, string creatorId);
        Task<bool> DeleteAnswerAsync(int answerId, string creatorId);
        Task<bool> UpdateQuestionAsync(int questionId, string createdById, string question);
        Task<bool> UpdateAnswerAsync(int answerId, string createdById, string answer);
        Task<bool> PinQuestionAsync(int questionId, bool isPinned);
    }
}
