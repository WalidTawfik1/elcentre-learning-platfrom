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
            // Get current user id (instructor)
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorId))
            {
                return BadRequest(new { message = "Instructor not found. Please sign in." });
            }
            var instructorName = User.FindFirstValue(ClaimTypes.GivenName);

            // Create notification entity
            var notification = new CourseNotification
            {
                Title = notificationDto.Title,
                Message = notificationDto.Message,
                CourseId = notificationDto.CourseId,
                CreatedById = instructorId,
                CreatedByName = instructorName ?? "Instructor",
                NotificationType = notificationDto.NotificationType,
                CreatedAt = DateTime.Now
            };

            // Create the notification
            var result = await _notificationService.CreateCourseNotificationAsync(notification);

            return Ok(result);
        }

        [HttpGet("get-course-notifications/{courseId}")]
        public async Task<IActionResult> GetCourseNotifications(int courseId, [FromQuery] bool unreadOnly = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetCourseNotificationsForUserAsync(userId, courseId, unreadOnly);
            return Ok(notifications);
        }

        [HttpPost("mark-notification-asread/{notificationId}")]
        public async Task<IActionResult> MarkCourseNotificationAsRead(int notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _notificationService.MarkCourseNotificationAsReadAsync(notificationId, userId);
            return Ok();
        }

        [HttpPost("mark-all-notifications-asread/{courseId}")]
        public async Task<IActionResult> MarkAllCourseNotificationsAsRead(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _notificationService.MarkAllCourseNotificationsAsReadAsync(userId, courseId);
            return Ok();
        }
    }
}