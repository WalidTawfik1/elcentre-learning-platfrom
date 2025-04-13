using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Services
{
    public interface IVideoService
    {
        Task<string> UploadVideoAsync(IFormFile file);
        Task<bool> DeleteVideoAsync(string publicId);
        Task<string> GetVideoUrl(string publicId);

    }
}
