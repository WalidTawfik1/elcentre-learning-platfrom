using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Services
{
    public interface IProfilePicture
    {
        Task<string?> AddImageAsync(IFormFileCollection files, string src);

        void DeleteImageAsync(string src);
    }
}
