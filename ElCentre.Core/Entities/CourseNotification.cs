using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{    public class CourseNotification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty; // Optional, can be used for display purposes
        public string CreatedById { get; set; } = string.Empty; // Creator ID (Instructor, Admin, System)
        public string CreatedByName { get; set; } = string.Empty; // Creator Name
        public string CreatorImage { get; set; } = string.Empty; // Optional, can be used for display purposes
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string NotificationType { get; set; } = string.Empty; // "NewLesson", "Announcement", "CourseApproved", "CourseRejected", etc.
        public string TargetUserRole { get; set; } = string.Empty; // "Student", "Instructor", "All"
        public string? TargetUserId { get; set; } // Specific user ID (optional)
        public bool IsGlobal { get; set; } = false; // Global notifications for all users
        public string Priority { get; set; } = "Normal"; // "Low", "Normal", "High", "Urgent"
        public DateTime? ExpiresAt { get; set; } // Optional expiration date
        public bool IsActive { get; set; } = true; // Can be used to soft delete notifications
    }

    public static class NotificationTypes
    {
        // Course-related notifications
        public const string NewLesson = "NewLesson";
        public const string Announcement = "Announcement";
        public const string CourseUpdate = "CourseUpdate";
        public const string QuizAvailable = "QuizAvailable";
        public const string GradePosted = "GradePosted";
        public const string AssignmentDue = "AssignmentDue";
        
        // Admin/Course status notifications
        public const string CourseApproved = "CourseApproved";
        public const string CourseRejected = "CourseRejected";
        public const string CoursePendingReview = "CoursePendingReview";
        
        // System notifications
        public const string Welcome = "Welcome";
        public const string SystemMaintenance = "SystemMaintenance";
        public const string AccountUpdated = "AccountUpdated";
        
        // Enrollment notifications
        public const string EnrollmentConfirmed = "EnrollmentConfirmed";
        public const string CertificateReady = "CertificateReady";
    }

    public static class NotificationPriority
    {
        public const string Low = "Low";
        public const string Normal = "Normal";
        public const string High = "High";
        public const string Urgent = "Urgent";
    }

    public static class TargetUserRoles
    {
        public const string Student = "Student";
        public const string Instructor = "Instructor";
        public const string Admin = "Admin";
        public const string All = "All";
    }
}
