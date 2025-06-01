using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Interfaces;
using ElCentre.Infrastructure.Data;
using ElCentre.Infrastructure.Repositories.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ElCentreDbContext _context;
        private readonly IMapper _mapper;
        private readonly IProfilePicture _profilePicture;

        public UserRepository(ElCentreDbContext context, IMapper mapper, IProfilePicture profilePicture)
        {
            _context = context;
            _mapper = mapper;
            _profilePicture = profilePicture;
        }

        public async Task<IEnumerable<UserDTO>> GetAllInstructorsAsync()
        {
            var instructors = await _context.Users
                .Where(u => u.UserType == "Instructor")
                .ToListAsync();
            if (instructors == null || !instructors.Any())
            {
                return new List<UserDTO>();
            }
            var result = _mapper.Map<IEnumerable<UserDTO>>(instructors);
            return result;

        }

        public async Task<UserDTO> GetInstructorById(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }
            var instructor = await _context.Users.FirstOrDefaultAsync(u => u.Id == Id && u.UserType == "Instructor");
            if (instructor == null)
            {
                throw new KeyNotFoundException($"Instructor with ID {Id} not found.");
            }
            var result = _mapper.Map<UserDTO>(instructor);
            return result;

        }

        public async Task<UserDTO> GetUserProfileAsync(string Id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null)
            {
                return null;
            }
            var result =  _mapper.Map<UserDTO>(user);
            return result;

        }

        public async Task<UpdateUserDTO> UpdateUserProfileAsync(string Id, UpdateUserDTO userDTO)
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == Id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {Id} not found.");
            }
            _mapper.Map(source: userDTO, user);

            if (userDTO.ProfilePicture != null)
            {
                if(user.ProfilePicture != null)
                {
                    _profilePicture.DeleteImageAsync(user.ProfilePicture);
                    var profilePicture = await _profilePicture.AddImageAsync(userDTO.ProfilePicture, user.Id);
                    user.ProfilePicture = profilePicture;
                }
                else
                {
                    var profilePicture = await _profilePicture.AddImageAsync(userDTO.ProfilePicture, user.Id);
                    user.ProfilePicture = profilePicture;
                }
            }

            await _context.SaveChangesAsync();

            return _mapper.Map<UpdateUserDTO>(user);

        }
    }
}
