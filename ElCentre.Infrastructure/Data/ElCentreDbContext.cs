using ElCentre.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Data
{
    public class ElCentreDbContext:IdentityDbContext<AppUser>
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CourseModule> CourseModules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }
        public DbSet<CompletedLesson> CompletedLessons { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<StudentQuiz> StudentQuizzes { get; set; }
        public DbSet<CourseNotification> CourseNotifications { get; set; }
        public DbSet<NotificationReadStatus> NotificationReadStatuses { get; set; }
        public DbSet<LessonQuestion> LessonQuestions { get; set; }
        public DbSet<LessonAnswer> LessonAnswers { get; set; }

        public ElCentreDbContext(DbContextOptions<ElCentreDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // User has many courses as an instructor
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(u => u.CreatedCourses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course has one category
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course has many modules
            modelBuilder.Entity<CourseModule>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);


            // Module has many lessons
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);


            // User has many enrollments
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);



            // Course has many enrollments
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);



            // User has many payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollment has many payments (optional)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Enrollment)
                .WithMany(e => e.Payments)
                .HasForeignKey(p => p.EnrollmentId)
                .IsRequired(false) // optional
                .OnDelete(DeleteBehavior.Restrict);

            // User has many reviews
            modelBuilder.Entity<CourseReview>()
                .HasOne(r => r.User)
                .WithMany(u => u.CourseReviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Course has many reviews
            modelBuilder.Entity<CourseReview>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course has many quizes
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Course)
                .WithMany(l => l.Quizzes)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lesson has many quizes
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Lesson)
                .WithMany(l => l.Quizzes)
                .HasForeignKey(q => q.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentQuiz>()
                .HasOne(sq => sq.Quiz)
                .WithMany(q => q.StudentQuizzes)
                .HasForeignKey(sq => sq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentQuiz>()
                .HasOne(sq => sq.Student)
                .WithMany(s => s.StudentQuizzes)
                .HasForeignKey(sq => sq.StudentId)
                .OnDelete(DeleteBehavior.Restrict);




        }
    }


}
