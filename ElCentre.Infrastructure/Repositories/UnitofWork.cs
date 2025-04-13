using AutoMapper;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class UnitofWork : IUnitofWork
    {
        private readonly ElCentreDbContext _context;
        private IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IGenerateToken _generateToken;
        private readonly IEmailService _emailService;
        private readonly ICourseThumbnailService _courseThumbnailService;
        private readonly IVideoService _videoService;

        public IAuthentication Authentication { get; }
        public ICategoryRepository CategoryRepository { get; }
        public IUserRepository UserRepository { get; }
        public ICourseRepository CourseRepository { get; }
        public ICourseModuleRepository CourseModuleRepository { get; }
        public ILessonRepository LessonRepository { get; }

        public UnitofWork(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IMapper mapper, ElCentreDbContext context, IGenerateToken generateToken, IEmailService emailService, ICourseThumbnailService courseThumbnailService, IVideoService videoService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _context = context;
            _generateToken = generateToken;
            _emailService = emailService;
            _courseThumbnailService = courseThumbnailService;
            _videoService = videoService;


            Authentication = new AuthenticationRepository(_userManager, _emailService, _signInManager, _generateToken, _context);
            CategoryRepository = new CategoryRepository(_context);
            UserRepository = new UserRepository(_context, _mapper);
            CourseRepository = new CourseRepository(_courseThumbnailService, _mapper, _context);
            CourseModuleRepository = new CourseModuleRepository(_context);
            LessonRepository = new LessonRepository(_context, _videoService);
        }

    }
}
