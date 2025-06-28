using ElCentre.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories.Services
{
    class CourseThumbnailService : ICourseThumbnailService
    {
        private readonly IFileProvider file;

        public CourseThumbnailService(IFileProvider file)
        {
            this.file = file;
        }

        public async Task<string?> AddImageAsync(IFormFileCollection files, string src)
        {
            if (files == null || files.Count == 0)
                return null;

            var file = files[0]; // take only the first file
            if (file.Length == 0)
                return null;

            // Validate file size (limit to 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size exceeds the 5MB limit");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png"};
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException($"File type {fileExtension} is not allowed");

            // Validate content type
            var allowedContentTypes = new[] { "image/jpeg", "image/png"};
            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                throw new ArgumentException($"Content type {file.ContentType} is not allowed");

            // Sanitize filename
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var safeFileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var finalFileName = $"{safeFileName}_{DateTime.UtcNow.Ticks}{fileExtension}";

            var imageDirectory = Path.Combine("wwwroot", "CourseThumbnails", src);
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            var imageSrc = $"/CourseThumbnails/{src}/{finalFileName}";
            var root = Path.Combine(imageDirectory, finalFileName);

            using (var stream = new FileStream(root, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return imageSrc;
        }

        public void DeleteImageAsync(string src)
        {
            if (string.IsNullOrEmpty(src))
                return;

            var info = file.GetFileInfo(src);
            var root = info.PhysicalPath;
            if (File.Exists(root))
            {
                File.Delete(root);
            }
        }
    }
}
