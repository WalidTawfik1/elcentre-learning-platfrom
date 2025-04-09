using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Sharing
{
   public static class GenerateCode
    {
        public static string GenerateOtpCode(int length)
        {
            Random random = new Random();

            string otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                otp += random.Next(0, 10).ToString();
            }

            return otp;
        }
    }
}
