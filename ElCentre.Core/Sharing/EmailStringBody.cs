using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Sharing
{
    public class EmailStringBody
    {
        public static string send(string email, string token, string component, string message)
        {
            string encodeToken = Uri.EscapeDataString(token);
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
        .button {{
            display: inline-block;
            padding: 12px 25px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            font-size: 16px;
            border-radius: 5px;
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
            <a href='http://localhost:3000/Account/{component}?email={email}&code={encodeToken}' class='button'>
                Activate Account
            </a>
            <div class='footer'>
                If you did not request this, please ignore this email.
            </div>
        </div>
    </div>
</body>
</html>
";

        }
    }
}
