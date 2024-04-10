namespace PEC_TrackerAdvisorAPI.Helpers
{
    public static class EmailBody
    {
        public static string ResetPasswordEmailBody(string email, string emailToken)
        {
            return $@"<html>
                        <head>
                        </head>
                        <body style=""margin:0;padding:0;font-family: Arial, Helvetica, sans-serif;"">
                            <div style=""height:auto;background: linear-gradient(to top, #c9c9ff 50%, #6e6ef6 90%) no-repeat; width:400px;padding:30px"">
                                <div>
                                    <div>
                                        <h1 style=""color: #fff; font-size: 24px; font-weight: 600; margin: 0;"">Password Reset</h1>
                                        <hr>
                                        <p> You are receiving this email because we received a password reset request for your account.</p>

                                        <p>Click the button below to reset your password:</p>

                                        <a href=""http://localhost:4200/reset-password?email={email}&token={emailToken}"" target=""_blank"" style=""background-color: #6e6ef6; color: #fff; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin: 10px 0; display: inline-block;"">Reset Password</a>
                                        <p>If you did not request a password reset, no further action is required.</p>
                                        <p>If you’re having trouble clicking the ""Reset Password"" button, copy and paste the URL below into your web browser:</p>
                                        <p>http://localhost:4200/reset-password?email={email}&token={emailToken}</p>
                                        <p>Best Regards,<br><br><b>PEC Tracker&Advisor Team</b></p>
                                    </div>
                                </div>
                            </div>
                        </body>
                    </html>";
        }
    }
}
