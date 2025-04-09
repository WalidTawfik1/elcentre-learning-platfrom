using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Sharing
{
    public class EmailStringBody
    {
        public static string send(string email, string otpCode, string message)
        {
            return $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f9f9f9;
            margin: 0;
            padding: 0;
        }}
        .container {{
            width: 100%;
            padding: 40px 0;
            display: flex;
            justify-content: center;
            align-items: center;
        }}
        .content {{
            background-color: #ffffff;
            width: 100%;
            max-width: 600px;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            text-align: center;
        }}
        .message {{
            font-size: 18px;
            color: #333333;
            margin-bottom: 30px;
        }}
        .otp-container {{
            margin: 25px 0;
        }}
        .otp-code {{
            font-size: 32px;
            font-weight: bold;
            letter-spacing: 5px;
            color: #007bff;
            padding: 15px 25px;
            background-color: #f0f7ff;
            border-radius: 5px;
            border: 1px dashed #007bff;
        }}
        .instructions {{
            margin-top: 20px;
            font-size: 16px;
            color: #555555;
        }}
        .footer {{
            margin-top: 30px;
            font-size: 12px;
            color: #999999;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='content'>
            <div class='message'>
                {message}
            </div>
            <div class='otp-container'>
                <div class='otp-code'>{otpCode}</div>
            </div>
            <div class='instructions'>
                Enter this verification code in the app to verify your account.
            </div>
            <div class='footer'>
                If you did not request this, please ignore this email.
                This code will expire in 10 minutes.
            </div>
        </div>
    </div>
</body>
</html>
";
        }
    }
}
