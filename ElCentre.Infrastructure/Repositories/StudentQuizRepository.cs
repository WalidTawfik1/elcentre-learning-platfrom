using AutoMapper;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class StudentQuizRepository : GenericRepository<StudentQuiz>, IStudentQuizRepository
    {
        private readonly ElCentreDbContext _context;

        public StudentQuizRepository(ElCentreDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<string> GetTotalQuizScoreAsync(string studentId, int lessonId)
        {
            var totalquizzes = await _context.Quizzes
                .Where(q => q.LessonId == lessonId)
                .CountAsync();
            // Efficiently compute the sum of scores using a single database query
            var totalscore = await _context.StudentQuizzes
                .Join(_context.Quizzes,
                    sq => sq.QuizId,
                    q => q.Id,
                    (sq, q) => new { StudentQuiz = sq, Quiz = q })
                .Where(x => x.StudentQuiz.StudentId == studentId && x.Quiz.LessonId == lessonId)
                .SumAsync(x => x.StudentQuiz.Score);
            var result = totalquizzes > 0 ? $"{totalscore}/{totalquizzes}" : "No quizzes";
            return result;

        }

        public async Task<StudentQuiz> SubmitQuizAnswerAsync(string studentId, int quizId, string answer)
        {
            // Get the quiz to check the correct answer
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null)
                throw new ArgumentException("Quiz not found", nameof(quizId));

            // Calculate score based on correct answer
            int score = answer.Equals(quiz.CorrectAnswer, StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            // Check if answer already exists
            var existingAnswer = await _context.StudentQuizzes
                .FirstOrDefaultAsync(sq => sq.StudentId == studentId && sq.QuizId == quizId);

            if (existingAnswer != null)
            {
                return null;
            }
            else
            {
                // Create new answer
                var studentQuiz = new StudentQuiz
                {
                    StudentId = studentId,
                    QuizId = quizId,
                    Answer = answer,
                    Score = score,
                    TakenAt = DateTime.Now
                };

                await _context.StudentQuizzes.AddAsync(studentQuiz);
                await _context.SaveChangesAsync();
                return studentQuiz;
            }
        }

        public async Task<IEnumerable<StudentQuiz>> GetStudentQuizzesByLessonAsync(string studentId, int lessonId)
        {
            // Get all quizzes answered by a student in a specific lesson
            return await _context.StudentQuizzes
                .Join(_context.Quizzes,
                    sq => sq.QuizId,
                    q => q.Id,
                    (sq, q) => new { StudentQuiz = sq, Quiz = q })
                .Where(x => x.StudentQuiz.StudentId == studentId && x.Quiz.LessonId == lessonId)
                .Select(x => x.StudentQuiz)
                .Include(sq => sq.Quiz)
                .ToListAsync();
        }
    }
}
