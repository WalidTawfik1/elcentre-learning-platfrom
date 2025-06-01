using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories.Services
{
    public interface IProfilePicture
    {
        Task<string> AddImageAsync(IFormFileCollection files, string src);

        void DeleteImageAsync(string src);
    }
}
