using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public record RegisterDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public UserType UserType { get; set; }
    }

    public record LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public record ResetPasswordDTO : LoginDTO
    {
        public string Token { get; set; }
    }

    public record ActiveAccountDTO
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
