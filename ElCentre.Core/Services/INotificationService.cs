using ElCentre.Core.Entities;

namespace ElCentre.Core.Services
{
    public interface INotificationService
    {
        Task<CourseNotification> CreateCourseNotificationAsync(CourseNotification notification);
        Task<List<CourseNotification>> GetCourseNotificationsForUserAsync(string userId, int courseId, bool unreadOnly = false);
        Task MarkCourseNotificationAsReadAsync(int notificationId, string userId);
        Task MarkAllCourseNotificationsAsReadAsync(string userId, int courseId);
    }
}