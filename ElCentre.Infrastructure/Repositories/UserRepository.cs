using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Core.Sharing;
using ElCentre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Polly;
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

        public async Task<bool> BlockAccount(string userId, bool block)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            user.IsActive = !block; // If block is true, set IsActive to false
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> DeleteAccount(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            user.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserDTO>> GetAllInstructorsAsync()
        {
            var instructors = await _context.Users
                .Where(u => u.UserType == "Instructor" 
                && u.IsActive == true
                && u.CreatedCourses.Any(c => c.CourseStatus == "Approved" && c.IsActive && !c.IsDeleted))
                .ToListAsync();
            if (instructors == null || !instructors.Any())
            {
                return new List<UserDTO>();
            }
            var result = _mapper.Map<IEnumerable<UserDTO>>(instructors);
            return result;

        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(PagenationParams pagenationParams)
        {
            var query = _context.Users.AsQueryable();

            //filter by search
            if (!string.IsNullOrEmpty(pagenationParams.search))
            {
                var rawSearchWords = pagenationParams.search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Replace ه with ة only when it's the last character of each word
                var normalizedSearchWords = rawSearchWords.Select(word =>
                    word.EndsWith('ه') ? word.Substring(0, word.Length - 1) + 'ة' : word
                ).ToArray();

                if (normalizedSearchWords.Any())
                {
                    // Match ANY of the search words (name/description)
                    query = query.Where(m =>
                        normalizedSearchWords.Any(word =>
                            m.UserName.ToLower().Contains(word.ToLower()) ||
                            m.Email.ToLower().Contains(word.ToLower()) ||
                            m.FirstName.ToLower().Contains(word.ToLower()) ||
                            m.LastName.ToLower().Contains(word.ToLower())
                        )
                    );
                }
            }

            if (!string.IsNullOrEmpty(pagenationParams.sort))
            {
                query = pagenationParams.sort switch
                {
                    "Admin" => query.Where(m => m.UserType == "Admin"),
                    "Instructor" => query.Where(m => m.UserType == "Instructor"),
                    "Student" => query.Where(m => m.UserType == "Student"),
                    "Active" => query.Where(m => m.IsActive == true),
                    "Blocked" => query.Where(m => m.IsActive == false),
                    _ => query.OrderByDescending(m => m.CreatedAt),
                };
            }
            if (pagenationParams.sort == null) query = query.OrderByDescending(m => m.CreatedAt);
            query = query.Skip((pagenationParams.pagenum - 1) * pagenationParams.pagesize).Take(pagenationParams.pagesize);
            var result = _mapper.Map<List<UserDTO>>(query);
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

        public async Task<UserDTO?> GetUserProfileAsync(string Id)
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

            var currentProfilePicture = user.ProfilePicture;

            _mapper.Map(source: userDTO, user);

            if (userDTO.ProfilePicture != null)
            {
                // Delete the old profile picture if it exists
                if (user.ProfilePicture != null)
                {
                    _profilePicture.DeleteImageAsync(user.ProfilePicture);
                }
                
                var profilePicture = await _profilePicture.AddImageAsync(userDTO.ProfilePicture, user.Id);
                if (profilePicture != null)
                {
                    user.ProfilePicture = profilePicture;
                }
            }
            else
            {
                user.ProfilePicture = currentProfilePicture; // Keep the old profile picture if no new one is provided
            }

                await _context.SaveChangesAsync();

            return _mapper.Map<UpdateUserDTO>(user);

        }
    }
}
