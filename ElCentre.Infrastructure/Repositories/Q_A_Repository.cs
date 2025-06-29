using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class Q_A_Repository : IQ_A
    {
        private readonly ElCentreDbContext _context;
        private readonly INotificationService _notification;

        public Q_A_Repository(ElCentreDbContext context, INotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        public async Task<bool> AddAnswerAsync(string answer, string createdById, string createdByName, string creatorImage, bool isInstructor, int questionId)
        {
            var answerEntity = new LessonAnswer
            {
                Answer = answer,
                CreatedById = createdById,
                CreatedByName = createdByName,
                CreatorImage = creatorImage,
                IsInstructor = isInstructor,
                QuestionId = questionId
            };
            await _context.LessonAnswers.AddAsync(answerEntity);
            // Notify the question creator about the new answer
                var question = await _context.LessonQuestions.FindAsync(questionId);
                var lesson = await _context.Lessons
                    .Include(l => l.Module.Course)
                    .FirstOrDefaultAsync(l => l.Id == question.LessonId);
            var course = await _context.Courses.FindAsync(lesson.Module.Course.Id);
            var notification = new CourseNotification
                {
                    Title = "New Answer Posted",
                    Message = $"{createdByName} has answered on {question.Question}: {answer}",
                    CourseId = course.Id,
                    CourseName = course.Title,
                    CreatedById = createdById,
                    CreatedByName = createdByName,
                    CreatorImage = creatorImage,
                    NotificationType = NotificationTypes.NewAnswer,
                    TargetUserId = question.CreatedById // Notify the question creator
                };
                await _notification.CreateCourseNotificationAsync(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddQuestionAsync(string question, string createdById, string createdByName, string creatorImage, bool isInstructor, int lessonId)
        {
            var questionEntity = new LessonQuestion
            {
                Question = question,
                CreatedById = createdById,
                CreatedByName = createdByName,
                CreatorImage = creatorImage,
                IsInstructor = isInstructor,
                LessonId = lessonId
            };
            await _context.LessonQuestions.AddAsync(questionEntity);
            // Notify the instructor about the new question
            if (!isInstructor)
            {
                var lesson = await _context.Lessons
                    .Include(l => l.Module.Course)
                    .FirstOrDefaultAsync(l => l.Id == lessonId);
                if (lesson != null)
                {
                    var course = await _context.Courses.FindAsync(lesson.Module.Course.Id);
                    if (course != null)
                    {
                        var notification = new CourseNotification
                        {
                            Title = "New Question Posted",
                            Message = $"{createdByName} has posted a new question in {lesson.Title} lesson: {question}",
                            CourseId = course.Id,
                            CourseName = course.Title,
                            CreatedById = createdById,
                            CreatedByName = createdByName,
                            CreatorImage = creatorImage,
                            NotificationType = NotificationTypes.NewAnswer,
                            TargetUserId = course.InstructorId // Notify the instructor
                        };
                        await _notification.CreateCourseNotificationAsync(notification);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAnswerAsync(int answerId, string creatorId)
        {
            var answer = await _context.LessonAnswers.Where(a => a.Id == answerId && a.CreatedById == creatorId).FirstOrDefaultAsync();
            if (answer == null)
            {
                return false; // Answer not found
            }
            _context.LessonAnswers.Remove(answer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuestionAsync(int questionId, string creatorId)
        {
            var question = await _context.LessonQuestions.Where(q => q.Id == questionId && q.CreatedById == creatorId).FirstOrDefaultAsync();
            if (question == null)
            {
                return false; // Question not found
            }
            _context.LessonQuestions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<LessonAnswer>> GetAnswersByQuestionIdAsync(int questionId)
        {
            var answers = await _context.LessonAnswers
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();
            return answers;

        }

        public async Task<IEnumerable<LessonQuestion>> GetQuestionsByLessonIdAsync(int lessonId)
        {
            var questions = await _context.LessonQuestions
                .Where(q => q.LessonId == lessonId)
                .ToListAsync();
            return questions;
        }

        public async Task<bool> UpdateAnswerAsync(int answerId, string createdById, string answer)
        {
            var answerEntity = await _context.LessonAnswers.Where(answer => answer.Id == answerId && answer.CreatedById == createdById).FirstOrDefaultAsync();
            if (answerEntity == null)
            {
                return false; // Answer not found
            }
            answerEntity.Answer = answer;
            answerEntity.IsEdited = true; // Mark as edited
            answerEntity.EditedAt = DateTime.UtcNow; // Update edited timestamp
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> UpdateQuestionAsync(int questionId, string createdById, string question)
        {
            var questionEntity = await _context.LessonQuestions.Where(question => question.Id == questionId && question.CreatedById == createdById).FirstOrDefaultAsync();
            if (questionEntity == null)
            {
                return false; // Question not found
            }
            questionEntity.Question = question;
            questionEntity.IsEdited = true; // Mark as edited
            questionEntity.EditedAt = DateTime.UtcNow; // Update edited timestamp
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
