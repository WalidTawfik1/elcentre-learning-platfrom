using ElCentre.Contracts.Hubs;
using ElCentre.Core.Entities;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ElCentreDbContext _dbContext;
        private readonly IHubContext<NotificationsHub> _hubContext;

        public NotificationService(ElCentreDbContext dbContext, IHubContext<NotificationsHub> hubContext)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
        }

        public async Task<CourseNotification> CourseStatusNotification(CourseNotification notification, string instructorId)
        {
            await _dbContext.CourseNotifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();

            await _hubContext.Clients.User(instructorId)
                .SendAsync("ReceiveCourseNotification", notification);

            return notification;
        }

        public async Task<CourseNotification> CreateCourseNotificationAsync(CourseNotification notification)
        {
            await _dbContext.CourseNotifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();

            // Send real-time notification to appropriate audience
            if (notification.IsGlobal)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveCourseNotification", notification);
            }
            else if (!string.IsNullOrEmpty(notification.TargetUserId))
            {
                await _hubContext.Clients.User(notification.TargetUserId)
                    .SendAsync("ReceiveCourseNotification", notification);
            }
            else
            {
                await _hubContext.Clients.Group($"course-{notification.CourseId}")
                    .SendAsync("ReceiveCourseNotification", notification);
            }

            return notification;
        }

        public async Task<List<CourseNotification>> GetCourseNotificationsForUserAsync(string userId, int courseId, bool unreadOnly = false)
        {
            var query = from n in _dbContext.CourseNotifications
                        where n.CourseId == courseId && n.IsActive &&
                              (n.TargetUserId == null || n.TargetUserId == userId) &&
                              (n.ExpiresAt == null || n.ExpiresAt > DateTime.Now)
                        join rs in _dbContext.NotificationReadStatuses
                        on new { NotificationId = n.Id, UserId = userId }
                        equals new { NotificationId = rs.NotificationId, UserId = rs.UserId }
                        into readStatus
                        from rs in readStatus.DefaultIfEmpty()
                        select new { Notification = n, IsRead = rs != null && rs.IsRead };

            if (unreadOnly)
            {
                query = query.Where(x => !x.IsRead);
            }

            var results = await query.OrderByDescending(x => x.Notification.CreatedAt).ToListAsync();
            return results.Select(x => x.Notification).ToList();
        }

        public async Task<List<CourseNotification>> GetAllNotificationsForUserAsync(string userId, bool unreadOnly = false, int page = 1, int pageSize = 20)
        {            // Get user's enrolled courses
            var enrolledCourseIds = await _dbContext.Enrollments
                .Where(e => e.StudentId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            // Get user's role
            var user = await _dbContext.Users.FindAsync(userId);
            var userRole = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            var query = from n in _dbContext.CourseNotifications
                        where n.IsActive &&
                              (n.ExpiresAt == null || n.ExpiresAt > DateTime.Now) &&
                              (n.IsGlobal ||
                               n.TargetUserId == userId ||
                               (n.TargetUserRole == "All") ||
                               (n.TargetUserRole == userRole) ||
                               (enrolledCourseIds.Contains(n.CourseId) && string.IsNullOrEmpty(n.TargetUserRole)))
                        join rs in _dbContext.NotificationReadStatuses
                        on new { NotificationId = n.Id, UserId = userId }
                        equals new { NotificationId = rs.NotificationId, UserId = rs.UserId }
                        into readStatus
                        from rs in readStatus.DefaultIfEmpty()
                        select new { Notification = n, IsRead = rs != null && rs.IsRead };

            if (unreadOnly)
            {
                query = query.Where(x => !x.IsRead);
            }

            var results = await query
                .OrderByDescending(x => x.Notification.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return results.Select(x => x.Notification).ToList();
        }

        public async Task<List<CourseNotification>> GetNotificationsByRoleAsync(string userRole, bool unreadOnly = false)
        {
            var query = _dbContext.CourseNotifications
                .Where(n => n.IsActive &&
                           (n.ExpiresAt == null || n.ExpiresAt > DateTime.Now) &&
                           (n.TargetUserRole == userRole || n.TargetUserRole == "All"));

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task MarkAllCourseNotificationsAsReadAsync(string userId, int courseId)
        {
            var courseNotifications = await _dbContext.CourseNotifications
                .Where(n => n.CourseId == courseId && n.IsActive)
                .ToListAsync();

            if (!courseNotifications.Any())
                return;

            var notificationIds = courseNotifications.Select(n => n.Id).ToList();
            var existingReadStatuses = await _dbContext.NotificationReadStatuses
                .Where(rs => rs.UserId == userId && notificationIds.Contains(rs.NotificationId))
                .ToListAsync();

            var existingStatusDict = existingReadStatuses.ToDictionary(rs => rs.NotificationId);
            var currentTime = DateTime.Now;

            foreach (var notification in courseNotifications)
            {
                if (existingStatusDict.TryGetValue(notification.Id, out var readStatus))
                {
                    if (!readStatus.IsRead)
                    {
                        readStatus.IsRead = true;
                        readStatus.ReadAt = currentTime;
                    }
                }
                else
                {
                    await _dbContext.NotificationReadStatuses.AddAsync(new NotificationReadStatus
                    {
                        NotificationId = notification.Id,
                        UserId = userId,
                        IsRead = true,
                        ReadAt = currentTime
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            var userNotifications = await GetAllNotificationsForUserAsync(userId, true); // Get unread only
            
            if (!userNotifications.Any())
                return;

            var notificationIds = userNotifications.Select(n => n.Id).ToList();
            var existingReadStatuses = await _dbContext.NotificationReadStatuses
                .Where(rs => rs.UserId == userId && notificationIds.Contains(rs.NotificationId))
                .ToListAsync();

            var existingStatusDict = existingReadStatuses.ToDictionary(rs => rs.NotificationId);
            var currentTime = DateTime.Now;

            foreach (var notification in userNotifications)
            {
                if (existingStatusDict.TryGetValue(notification.Id, out var readStatus))
                {
                    if (!readStatus.IsRead)
                    {
                        readStatus.IsRead = true;
                        readStatus.ReadAt = currentTime;
                    }
                }
                else
                {
                    await _dbContext.NotificationReadStatuses.AddAsync(new NotificationReadStatus
                    {
                        NotificationId = notification.Id,
                        UserId = userId,
                        IsRead = true,
                        ReadAt = currentTime
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkCourseNotificationAsReadAsync(int notificationId, string userId)
        {
            var readStatus = await _dbContext.NotificationReadStatuses
                .FirstOrDefaultAsync(rs => rs.NotificationId == notificationId && rs.UserId == userId);

            if (readStatus == null)
            {
                readStatus = new NotificationReadStatus
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    IsRead = true,
                    ReadAt = DateTime.Now
                };
                await _dbContext.NotificationReadStatuses.AddAsync(readStatus);
            }
            else
            {
                readStatus.IsRead = true;
                readStatus.ReadAt = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            var userNotifications = await GetAllNotificationsForUserAsync(userId, true);
            return userNotifications.Count;
        }

        public async Task<int> GetUnreadCourseNotificationCountAsync(string userId, int courseId)
        {
            var courseNotifications = await GetCourseNotificationsForUserAsync(userId, courseId, true);
            return courseNotifications.Count;        }

        // Automatic notification methods
        public async Task NotifyNewLessonAsync(int courseId, string lessonTitle, string instructorId, string instructorName)
        {
            var course = await _dbContext.Courses.FindAsync(courseId);
            var creator = await _dbContext.Users.FindAsync(instructorId);
            var notification = new CourseNotification
            {
                Title = "New Lesson Available",
                Message = $"A new lesson '{lessonTitle}' has been added to the course.",
                CourseId = courseId,
                CourseName = course?.Title ?? "Unknown Course",
                CreatedById = instructorId,
                CreatedByName = instructorName,
                CreatorImage = creator?.ProfilePicture ?? "https://i.ibb.co/1GpftgH6/training.png",
                NotificationType = NotificationTypes.NewLesson,
                TargetUserRole = TargetUserRoles.Student,
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTime.Now
            };

            await CreateCourseNotificationAsync(notification);
        }

        public async Task NotifyCourseStatusChangeAsync(int courseId, string status, string instructorId, string adminId, string adminName, string? reason = null)
        {
            var course = await _dbContext.Courses.FindAsync(courseId);
            var creator = await _dbContext.Users.FindAsync(adminId);
            var title = status.ToLower() switch
            {
                "approve" => "Course Approved",
                "reject" => "Course Rejected",
                "pending" => "Course Under Review",
                _ => "Course Status Updated"
            };

            var message = status.ToLower() switch
            {
                "approve" => $"Your course '{course?.Title}' has been approved and is now available to students.",
                "reject" => $"Your course '{course?.Title}' has been rejected. {(string.IsNullOrEmpty(reason) ? "" : $"Reason: {reason}")}",
                "pending" => $"Your course '{course?.Title}' is now under review by administrators.",
                _ => $"Your course '{course?.Title}' status has been updated to {status}."
            };

            var notificationType = status.ToLower() switch
            {
                "approve" => NotificationTypes.CourseApproved,
                "reject" => NotificationTypes.CourseRejected,
                "pending" => NotificationTypes.CoursePendingReview,
                _ => NotificationTypes.CourseUpdate
            };

            var priority = status.ToLower() switch
            {
                "approve" => NotificationPriority.High,
                "reject" => NotificationPriority.High,
                _ => NotificationPriority.Normal
            }; 
            var notification = new CourseNotification
            {
                Title = title,
                Message = message,
                CourseId = courseId,
                CourseName = course?.Title ?? "Unknown Course",
                CreatedById = adminId,
                CreatedByName = adminName,
                CreatorImage = creator?.ProfilePicture ?? "https://i.ibb.co/YFr894B3/admin.png",
                NotificationType = notificationType,
                TargetUserId = instructorId,
                Priority = priority,
                CreatedAt = DateTime.Now
            };

            await CreateCourseNotificationAsync(notification);
        }

        public async Task NotifyEnrollmentAsync(int courseId, string studentId, string studentName, string instructorId)
        {
            var course = await _dbContext.Courses.FindAsync(courseId);
              // Notify instructor about new enrollment
            var notification = new CourseNotification
            {
                Title = "New Student Enrollment",
                Message = $"{studentName} has enrolled in your course '{course?.Title}'.",
                CourseId = courseId,
                CreatedById = "System",
                CreatedByName = "System",
                NotificationType = NotificationTypes.EnrollmentConfirmed,
                TargetUserId = instructorId,
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTime.Now
            };

            await CreateCourseNotificationAsync(notification);            // Notify student about successful enrollment
            var studentNotification = new CourseNotification
            {
                Title = "Enrollment Confirmed",
                Message = $"You have successfully enrolled in '{course?.Title}'. Welcome to the course!",
                CourseId = courseId,
                CreatedById = "System",
                CreatedByName = "System",
                NotificationType = NotificationTypes.EnrollmentConfirmed,
                TargetUserId = studentId,
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTime.Now
            };

            await CreateCourseNotificationAsync(studentNotification);
        }

        public async Task<List<CourseNotification>> GetNotificationHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbContext.CourseNotifications
                .Where(n => (n.TargetUserId == userId || n.IsGlobal || string.IsNullOrEmpty(n.TargetUserId)));

            if (fromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(n => n.CreatedAt <= toDate.Value);

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task DeleteExpiredNotificationsAsync()
        {
            var expiredNotifications = await _dbContext.CourseNotifications
                .Where(n => n.ExpiresAt.HasValue && n.ExpiresAt < DateTime.Now)
                .ToListAsync();

            if (expiredNotifications.Any())
            {
                _dbContext.CourseNotifications.RemoveRange(expiredNotifications);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteNotificationAsync(int notificationId, string userId)
        {
            var notification = await _dbContext.CourseNotifications.FindAsync(notificationId);
            if (notification != null && notification.CreatedById == userId)
            {
                notification.IsActive = false; // Soft delete
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}