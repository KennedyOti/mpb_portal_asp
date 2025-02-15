using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MPB_PORTAL_WEB_APP.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string smtpServer = "smtp.gmail.com"; // Gmail SMTP
        private readonly int smtpPort = 587; // TLS Port
        private readonly string smtpUser = "kennedyotieno3750@gmail.com";  // Your email
        private readonly string smtpPass = "kqhj kxyg qwha olou"; // Use app password

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true; // Ensure SSL is enabled

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
