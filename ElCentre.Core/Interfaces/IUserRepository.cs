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
        Task<UpdateUserDTO> UpdateUserProfileAsync(string Id, UpdateUserDTO userDTO);
        Task<IEnumerable<UserDTO>> GetAllInstructorsAsync();
        Task<UserDTO> GetInstructorById(string Id);

    }
}
