using ElCentre.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using ElCentre.Core.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ElCentre.Contracts.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        private readonly INotificationService _notificationService;

        public NotificationsHub(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Connection management
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                // Optionally join user to personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Method to add a student to a course group
        public async Task JoinCourseGroup(string courseId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"course-{courseId}");
        }

        // Method to remove a student from a course group
        public async Task LeaveCourseGroup(string courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"course-{courseId}");
        }

        // Join multiple course groups at once
        public async Task JoinCourseGroups(List<string> courseIds)
        {
            foreach (var courseId in courseIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"course-{courseId}");
            }
        }

        // Leave multiple course groups at once
        public async Task LeaveCourseGroups(List<string> courseIds)
        {
            foreach (var courseId in courseIds)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"course-{courseId}");
            }
        }

        // Course notifications methods
        public async Task<List<CourseNotification>> GetCourseNotifications(string userId, int courseId, bool unreadOnly = false)
        {
            return await _notificationService.GetCourseNotificationsForUserAsync(userId, courseId, unreadOnly);
        }

        public async Task<List<CourseNotification>> GetAllNotifications(string userId, bool unreadOnly = false, int page = 1, int pageSize = 20)
        {
            return await _notificationService.GetAllNotificationsForUserAsync(userId, unreadOnly, page, pageSize);
        }

        public async Task<int> GetUnreadCount(string userId)
        {
            return await _notificationService.GetUnreadNotificationCountAsync(userId);
        }

        public async Task<int> GetCourseUnreadCount(string userId, int courseId)
        {
            return await _notificationService.GetUnreadCourseNotificationCountAsync(userId, courseId);
        }

        public async Task MarkCourseNotificationAsRead(int notificationId, string userId)
        {
            await _notificationService.MarkCourseNotificationAsReadAsync(notificationId, userId);
            
            // Notify user about read status update
            await Clients.User(userId).SendAsync("NotificationMarkedAsRead", notificationId);
        }

        public async Task MarkAllCourseNotificationsAsRead(string userId, int courseId)
        {
            await _notificationService.MarkAllCourseNotificationsAsReadAsync(userId, courseId);
            
            // Notify user about bulk read status update
            await Clients.User(userId).SendAsync("AllCourseNotificationsMarkedAsRead", courseId);
        }

        public async Task MarkAllNotificationsAsRead(string userId)
        {
            await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            
            // Notify user about all notifications marked as read
            await Clients.User(userId).SendAsync("AllNotificationsMarkedAsRead");
        }

        public async Task<CourseNotification> CreateCourseNotification(CourseNotification notification)
        {
            // Verify user has permission to create notifications for this course
            var currentUserId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = Context.User?.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(currentUserId) || 
                (userRole != "Instructor" && userRole != "Admin"))
            {
                throw new HubException("Unauthorized to create notifications");
            }

            notification.CreatedById = currentUserId;
            notification.CreatedByName = Context.User?.FindFirstValue(ClaimTypes.GivenName) ?? "Unknown";
            
            return await _notificationService.CreateCourseNotificationAsync(notification);
        }

        // Get notification history
        public async Task<List<CourseNotification>> GetNotificationHistory(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _notificationService.GetNotificationHistoryAsync(userId, fromDate, toDate);
        }

        // Method for instructors to send new lesson notifications
        public async Task NotifyNewLesson(int courseId, string lessonTitle)
        {
            var instructorId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var instructorName = Context.User?.FindFirstValue(ClaimTypes.GivenName) ?? "Instructor";
            
            if (string.IsNullOrEmpty(instructorId))
            {
                throw new HubException("User not authenticated");
            }

            await _notificationService.NotifyNewLessonAsync(courseId, lessonTitle, instructorId, instructorName);
        }

        // Method for admins to send course status notifications
        public async Task NotifyCourseStatus(int courseId, string status, string instructorId, string? reason = null)
        {
            var adminId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminName = Context.User?.FindFirstValue(ClaimTypes.GivenName) ?? "Admin";
            var userRole = Context.User?.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(adminId) || userRole != "Admin")
            {
                throw new HubException("Unauthorized to send course status notifications");
            }

            await _notificationService.NotifyCourseStatusChangeAsync(courseId, status, instructorId, adminId, adminName, reason);
        }

        // Method to delete notification (soft delete)
        public async Task DeleteNotification(int notificationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            await _notificationService.DeleteNotificationAsync(notificationId, userId);
            
            // Notify about deletion
            await Clients.User(userId).SendAsync("NotificationDeleted", notificationId);
        }
    }
}