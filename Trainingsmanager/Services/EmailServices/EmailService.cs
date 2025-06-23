using MailKit.Net.Smtp;
using MimeKit;

namespace Trainingsmanager.Services.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly string _from = "trainings.manager.info@gmail.com";
        private readonly string _appPassword = "zxulufswsadkhfoo";
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;

        public async Task SendEmailAsync(string toEmail, string subject, string messageBody)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_from));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = messageBody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_from, _appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
