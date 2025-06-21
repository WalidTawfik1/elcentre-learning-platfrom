using ElCentre.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using ElCentre.Core.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ElCentre.Contracts.Hubs
{
    public class NotificationsHub : Hub
    {
        private readonly INotificationService _notificationService;

        public NotificationsHub(INotificationService notificationService)
        {
            _notificationService = notificationService;
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

        // Course notifications methods
        public async Task<List<CourseNotification>> GetCourseNotifications(string userId, int courseId, bool unreadOnly = false)
        {
            return await _notificationService.GetCourseNotificationsForUserAsync(userId, courseId, unreadOnly);
        }

        public async Task MarkCourseNotificationAsRead(int notificationId, string userId)
        {
            await _notificationService.MarkCourseNotificationAsReadAsync(notificationId, userId);
        }

        public async Task MarkAllCourseNotificationsAsRead(string userId, int courseId)
        {
            await _notificationService.MarkAllCourseNotificationsAsReadAsync(userId, courseId);
        }

        public async Task<CourseNotification> CreateCourseNotification(CourseNotification notification)
        {
            return await _notificationService.CreateCourseNotificationAsync(notification);
        }
    }
}