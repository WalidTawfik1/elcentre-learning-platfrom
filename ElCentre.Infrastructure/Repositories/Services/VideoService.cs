using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ElCentre.Core.DTO;
using ElCentre.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories.Services
{
    public class VideoService : IVideoService
    {
        private readonly Cloudinary _cloudinary;

        public VideoService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<bool> DeleteVideoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Video
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            throw new Exception("Video deletion failed.");
        }

        public Task<string> GetVideoUrl(string publicId)
        {
            var url = _cloudinary.Api.UrlVideoUp
                    .Secure(true)
                    .Action("upload")
                    .BuildUrl(publicId);

            return Task.FromResult(url);
        }

        public async Task<string> UploadVideoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");  

            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = Guid.NewGuid().ToString(),
                EagerTransforms = new List<Transformation>
                {
                  new EagerTransformation().Width(720).Height(480).Crop("fit"),
                  new EagerTransformation().Width(160).Height(90).Crop("fill").AudioCodec("none")
                },
                EagerAsync = true,
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString();
            }

            throw new Exception("Video upload failed.");
        }
    }
}
