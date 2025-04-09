using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Interfaces;
using ElCentre.Infrastructure.Data;
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

        public UserRepository(ElCentreDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<UserDTO> UpdateUserProfileAsync(string Id, UserDTO userDTO)
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
            _mapper.Map(userDTO, user);

            await _context.SaveChangesAsync();

            return _mapper.Map<UserDTO>(user);

        }
    }
}
