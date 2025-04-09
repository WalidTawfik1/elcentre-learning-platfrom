using ElCentre.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<UserDTO> GetUserProfileAsync(string Id);
        Task<UserDTO> UpdateUserProfileAsync(string Id, UserDTO userDTO);
    }
}
