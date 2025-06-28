using ElCentre.Core.Entities;
using ElCentre.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories.Services
{
    public class GenerateToken : IGenerateToken
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;

        public GenerateToken(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public string GetAndCreateToken(AppUser user)
        {
            var role = _userManager.GetRolesAsync(user).Result;
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim()),
                new Claim("ProfilePicture", user.ProfilePicture?? "https://drive.google.com/uc?export=view&id=1KktjLJLwGQvMfsD8hkcpoApe0YYbmcO_")
            };
            foreach (var item in role)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }
            var security = _configuration["Token:Secret"];
            var key = Encoding.ASCII.GetBytes(security);
            var SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(int.Parse(_configuration["Token:ExpiryDays"])),
                Issuer = _configuration["Token:Issuer"],
                SigningCredentials = SigningCredentials,
                NotBefore = DateTime.Now
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
    }
}
