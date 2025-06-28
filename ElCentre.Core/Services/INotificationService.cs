using ElCentre.Core.Entities;

namespace ElCentre.Core.Services
{
    public interface INotificationService
    {
        // Course notifications
        Task<CourseNotification> CreateCourseNotificationAsync(CourseNotification notification);
        Task<CourseNotification> CourseStatusNotification(CourseNotification notification, string instructorId);
        Task<List<CourseNotification>> GetCourseNotificationsForUserAsync(string userId, int courseId, bool unreadOnly = false);
        
        // Global notifications
        Task<List<CourseNotification>> GetAllNotificationsForUserAsync(string userId, bool unreadOnly = false, int page = 1, int pageSize = 20);
        Task<List<CourseNotification>> GetNotificationsByRoleAsync(string userRole, bool unreadOnly = false);
        
        // Notification management
        Task MarkCourseNotificationAsReadAsync(int notificationId, string userId);
        Task MarkAllCourseNotificationsAsReadAsync(string userId, int courseId);
        Task MarkAllNotificationsAsReadAsync(string userId);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task<int> GetUnreadCourseNotificationCountAsync(string userId, int courseId);
        
        // Automatic notifications
        Task NotifyNewLessonAsync(int courseId, string lessonTitle, string instructorId, string instructorName);
        Task NotifyCourseStatusChangeAsync(int courseId, string status, string instructorId, string adminId, string adminName, string? reason = null);
        Task NotifyEnrollmentAsync(int courseId, string studentId, string studentName, string instructorId);
        
        // History and cleanup
        Task<List<CourseNotification>> GetNotificationHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task DeleteExpiredNotificationsAsync();
        Task DeleteNotificationAsync(int notificationId, string userId);
    }
}