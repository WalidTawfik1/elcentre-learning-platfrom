using AutoMapper;
using CloudinaryDotNet;
using ElCentre.Core.DTO;
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
    public class PendingCourseRepository : IPendingCourseRepository
    {
        private readonly ElCentreDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public PendingCourseRepository(ElCentreDbContext context, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<IEnumerable<CourseDTO>> GetAllPendingCoursesAsync()
        {
            var pendingCourses = await _context.Courses
                .Where(c => c.CourseStatus == "Pending")
                .ToListAsync();
            if (pendingCourses == null || !pendingCourses.Any())
            {
                return new List<CourseDTO>();
            }
            var result = _mapper.Map<List<CourseDTO>>(pendingCourses);
            return result ;
        }

        public async Task<CourseDTO> GetPendingCourseByIdAsync(int courseId)
        {
            var pendingCourse = await _context.Courses
                .Where(c => c.CourseStatus == "Pending" && c.Id == courseId)
                .FirstOrDefaultAsync();
            if (pendingCourse == null)
            {
                throw new KeyNotFoundException($"Pending course with ID {courseId} not found.");
            }
            var result = _mapper.Map<CourseDTO>(pendingCourse);
            return result;
        }

        public async Task<bool> UpdatePendingCourseAsync(int courseId, string decision, string? rejectionReason)
        {
            var pendingCourse = await _context.Courses
                .Include(pc => pc.Instructor)
                .FirstOrDefaultAsync(pc => pc.Id == courseId);
            if (pendingCourse == null)
            {
                throw new KeyNotFoundException($"Pending course with ID {courseId} not found.");
            }
            if (decision.Equals("approve", StringComparison.OrdinalIgnoreCase))
            {
                pendingCourse.CourseStatus = "Approved";
                var instructor = pendingCourse.Instructor;
                var contnet = $@"
                    <html>
                    <head>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                color: #333333;
                            }}
                            .container {{
                                padding: 20px;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 14px;
                                color: #777777;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <p>Dear {instructor.FirstName},</p>
                    
                            <p>We are pleased to inform you that your course titled <strong>'{pendingCourse.Title}'</strong> has been reviewed and approved for publishing on ElCentre.</p>
                    
                            <p>Thank you for your valuable contribution. We look forward to seeing your course inspire many learners!</p>
                    
                            <div class='footer'>
                                <p>Best regards,<br>The ElCentre Team</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var emaildto = new EmailDTO(
                    instructor.Email,
                   "elcentre.business@gmail.com",
                   "Course Approved",
                   contnet);
                await _emailService.SendEmailAsync(emaildto);

            }
            else if (decision.Equals("reject", StringComparison.OrdinalIgnoreCase))
            {
                pendingCourse.CourseStatus = "Rejected";
                var instructor = pendingCourse.Instructor;
                var contnet = $@"
                    <html>
                    <head>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                color: #333333;
                            }}
                            .container {{
                                padding: 20px;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 14px;
                                color: #777777;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <p>Dear {instructor.FirstName},</p>
                    
                            <p>We regret to inform you that your course titled <strong>'{pendingCourse.Title}'</strong> has not been approved for publishing on ElCentre at this time.</p>
                    
                            <p><strong>Reason for rejection:</strong> {rejectionReason}</p>
                    
                            <p>You are encouraged to review the feedback and make the necessary updates. Once done, you may resubmit the course for another review.</p>
                    
                            <div class='footer'>
                                <p>Best regards,<br>The ElCentre Team</p>
                            </div>
                        </div>
                    </body>
                    </html>";
                var emaildto = new EmailDTO(
                    instructor.Email,
                   "elcentre.business@gmail.com",
                   "Course Rejected",
                   contnet);
                await _emailService.SendEmailAsync(emaildto);
            }
            else
            {
                throw new ArgumentException("Invalid decision. Use 'approve' or 'reject'.");
            }
            _context.Courses.Update(pendingCourse);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
