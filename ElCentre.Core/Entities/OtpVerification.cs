using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class OtpVerification
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsUsed { get; set; }
        public string Purpose { get; set; } = "VerifyEmail";
    }
}
