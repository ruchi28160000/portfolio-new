// filepath: c:\Users\Ruchi\PortfolioTrackerApi\Services\EmailVerificationService.cs
using System;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PortfolioTrackerApi.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly ConcurrentDictionary<string, VerificationEntry> _verificationCodes = new();
        private readonly TimeSpan _codeExpiration = TimeSpan.FromMinutes(5); // 5 minutes
        private readonly string _sendGridApiKey;

        public EmailVerificationService(IConfiguration configuration)
        {
            _sendGridApiKey = configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(_sendGridApiKey))
            {
                throw new Exception("SendGrid API key is not configured.");
            }
        }

        public async Task SendVerificationCode(string email)
        {
            if (!IsValidEmail(email))
                throw new ArgumentException("Invalid email address.");

            if (VerifiedEmailStore.IsEmailVerified(email))
                throw new ArgumentException("Email is already verified.");

            var code = new Random().Next(100000, 999999).ToString();

            var entry = new VerificationEntry
            {
                Code = code,
                CreatedAt = DateTime.UtcNow
            };

            _verificationCodes[email] = entry;
            await SendEmail(email, code);

            Console.WriteLine($"[Email to {email}]: Your verification code is {code}");
        }

        public bool VerifyCode(string email, string code)
        {
            if (VerifiedEmailStore.IsEmailVerified(email))
                return true;

            if (_verificationCodes.TryGetValue(email, out var entry))
            {
                if (DateTime.UtcNow - entry.CreatedAt > _codeExpiration)
                {
                    _verificationCodes.TryRemove(email, out _);
                    return false;
                }

                if (entry.Code == code)
                {
                    _verificationCodes.TryRemove(email, out _);
                    VerifiedEmailStore.SaveVerifiedEmail(email);
                    return true;
                }
            }

            return false;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task SendEmail(string toEmail, string code)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress("fintrackblr@gmail.com", "FinTrack");
            var subject = "Your FinTrack Verification Code";
            var to = new EmailAddress(toEmail);
            var plainTextContent =
                $"Hi Investor,\n\n" +
                "Welcome to FinTrack - Where Finance Meets Clarity.\n" +
                $"Your verification code is : {code}.\n" +
                "(This code is valid for 5 minutes.)\n\n" +
                "Please use this code to complete your verification.\n\n" +
                "Stay on top of your finances,\n" +
                "The FinTrack Team";
            var htmlContent =
                $"<p style='color:#000000;'>Hi Investor,</p>" +
                $"<p style='color:#000000;'><strong>Welcome to FinTrack - Where Finance Meets Clarity.</strong></p>" +
                $"<p style='color:#000000;'>Your verification code is : <strong>{code}</strong>.<br>(This code is valid for 5 minutes.)</p>" +
                $"<p style='color:#000000;'>Please use this code to complete your verification.</p>" +
                $"<p style='color:#000000;'>Stay on top of your finances,<br>The FinTrack Team</p>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"Failed to send email: {errorBody}");
            }
        }

        private class VerificationEntry
        {
            public string Code { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}