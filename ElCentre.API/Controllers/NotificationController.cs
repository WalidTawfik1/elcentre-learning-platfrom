using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ElCentre.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("create-course-notification")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateCourseNotification(CourseNotificationDTO notificationDto)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorId))
            {
                return BadRequest(new { message = "Instructor not found. Please sign in." });
            }
            var instructorName = User.FindFirstValue(ClaimTypes.GivenName);
            var instructorImage = User.FindFirstValue("ProfilePicture");
            var notification = new CourseNotification
            {
                Title = notificationDto.Title,
                Message = notificationDto.Message,
                CourseId = notificationDto.CourseId,
                CourseName = notificationDto.CourseName ?? "Unknown Course",
                CreatedById = instructorId,
                CreatedByName = instructorName ?? "Instructor",
                CreatorImage = instructorImage,
                NotificationType = notificationDto.NotificationType,
                TargetUserRole = TargetUserRoles.Student,
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTime.Now
            };

            var result = await _notificationService.CreateCourseNotificationAsync(notification);
            return Ok(result);
        }        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseNotifications(int courseId, [FromQuery] bool unreadOnly = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");
                
            var notifications = await _notificationService.GetCourseNotificationsForUserAsync(userId, courseId, unreadOnly);
            return Ok(notifications);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllNotifications(
            [FromQuery] bool unreadOnly = false, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");
                
            var notifications = await _notificationService.GetAllNotificationsForUserAsync(userId, unreadOnly, page, pageSize);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");
                
            var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
            return Ok(new { unreadCount = count });
        }

        [HttpGet("course/{courseId}/unread-count")]
        public async Task<IActionResult> GetCourseUnreadCount(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");
                
            var count = await _notificationService.GetUnreadCourseNotificationCountAsync(userId, courseId);
            return Ok(new { unreadCount = count });
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");
                
            await _notificationService.MarkCourseNotificationAsReadAsync(notificationId, userId);
            return Ok(new { message = "Notification marked as read" });
        }

        [HttpPut("course/{courseId}/read-all")]
        public async Task<IActionResult> MarkAllCourseNotificationsAsRead(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");
                
            await _notificationService.MarkAllCourseNotificationsAsReadAsync(userId, courseId);
            return Ok(new { message = "All course notifications marked as read" });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");
                
            await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            return Ok(new { message = "All notifications marked as read" });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetNotificationHistory(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");
                
            var history = await _notificationService.GetNotificationHistoryAsync(userId, fromDate, toDate);
            return Ok(history);
        }

        [HttpDelete("{notificationId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");
                
            await _notificationService.DeleteNotificationAsync(notificationId, userId);
            return Ok(new { message = "Notification deleted" });
        }

        // Endpoint for admin to send course status notifications
        [HttpPost("course-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendCourseStatusNotification([FromBody] CourseStatusNotificationRequest request)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized("Admin not found");
                
            var adminName = User.FindFirstValue(ClaimTypes.GivenName) ?? "Admin";

            await _notificationService.NotifyCourseStatusChangeAsync(
                request.CourseId, 
                request.Status, 
                request.InstructorId, 
                adminId, 
                adminName, 
                request.Reason);

            return Ok(new { message = "Course status notification sent" });
        }

        // Manual endpoint to trigger new lesson notification (for testing or manual triggers)
        [HttpPost("new-lesson")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> SendNewLessonNotification([FromBody] NewLessonNotificationRequest request)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized("Instructor not found");
                
            var instructorName = User.FindFirstValue(ClaimTypes.GivenName) ?? "Instructor";

            await _notificationService.NotifyNewLessonAsync(
                request.CourseId, 
                request.LessonTitle, 
                instructorId, 
                instructorName);

            return Ok(new { message = "New lesson notification sent" });
        }

        // Cleanup expired notifications (admin only)
        [HttpPost("cleanup-expired")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupExpiredNotifications()
        {
            await _notificationService.DeleteExpiredNotificationsAsync();
            return Ok(new { message = "Expired notifications cleaned up" });
        }
    }

    // Request models
    public class CourseStatusNotificationRequest
    {
        public int CourseId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string InstructorId { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class NewLessonNotificationRequest
    {
        public int CourseId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
    }
}