using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Infrastructure.Data;
using ElCentre.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        private readonly ElCentreDbContext _context;
        private readonly IMapper _mapper;
        public QuizRepository(ElCentreDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        async Task<IEnumerable<QuizDTO>> IQuizRepository.GetAllCourseQuizzesAsync(int CourseId)
        {
            var Quizzes = await _context.Quizzes
                .Where(q => q.CourseId == CourseId)
                .ToListAsync();
            var result = _mapper.Map<List<QuizDTO>>(Quizzes);
            return result;
        }
    }
}
