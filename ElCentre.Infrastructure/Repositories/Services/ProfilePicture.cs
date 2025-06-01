using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories.Services
{
    public class ProfilePicture : IProfilePicture
    {
        private readonly IFileProvider file;

        public ProfilePicture(IFileProvider file)
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

            var imageDirectory = Path.Combine("wwwroot", "ProfilePictures", src);
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            var imageName = file.FileName;
            var imageSrc = $"/ProfilePictures/{src}/{imageName}";
            var root = Path.Combine(imageDirectory, imageName);

            using (var stream = new FileStream(root, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return imageSrc;
        }


        public void DeleteImageAsync(string src)
        {
            var info = file.GetFileInfo(src);
            var root = info.PhysicalPath;
            if (File.Exists(root))
            {
                File.Delete(root);
            }
        }
    }
}
