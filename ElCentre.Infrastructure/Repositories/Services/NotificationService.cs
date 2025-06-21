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
            // Add the notification to the database
            await _dbContext.CourseNotifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();

            // Send real-time notification via SignalR to all online users in the course group
            await _hubContext.Clients.Group($"course-{notification.CourseId}")
                .SendAsync("ReceiveCourseNotification", notification);

            return notification;
        }

        public async Task<List<CourseNotification>> GetCourseNotificationsForUserAsync(string userId, int courseId, bool unreadOnly = false)
        {
            var query = from n in _dbContext.CourseNotifications
                        where n.CourseId == courseId
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

        public async Task MarkAllCourseNotificationsAsReadAsync(string userId, int courseId)
        {
            // Get all course notifications for the specified course
            var courseNotifications = await _dbContext.CourseNotifications
                .Where(n => n.CourseId == courseId)
                .ToListAsync();

            if (!courseNotifications.Any())
                return;

            // Get existing read statuses for this user and these notifications
            var notificationIds = courseNotifications.Select(n => n.Id).ToList();
            var existingReadStatuses = await _dbContext.NotificationReadStatuses
                .Where(rs => rs.UserId == userId && notificationIds.Contains(rs.NotificationId))
                .ToListAsync();

            // Create a dictionary for faster lookup
            var existingStatusDict = existingReadStatuses.ToDictionary(rs => rs.NotificationId);
            var currentTime = DateTime.Now;

            // For each notification, ensure a read status exists and is marked as read
            foreach (var notification in courseNotifications)
            {
                if (existingStatusDict.TryGetValue(notification.Id, out var readStatus))
                {
                    // Update existing read status if not already read
                    if (!readStatus.IsRead)
                    {
                        readStatus.IsRead = true;
                        readStatus.ReadAt = currentTime;
                    }
                }
                else
                {
                    // Create new read status
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
    }
}