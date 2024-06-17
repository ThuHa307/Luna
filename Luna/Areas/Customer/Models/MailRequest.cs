using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Luna.Areas.Customer.Models
{
    public class MailRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
    public class MailSettings
    {
        public string? Mail { get; set; }
        public string? DisplayName { get; set; }
        public string? Password { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
    }
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            //var email = new MimeMessage();
            //email.Sender = MailboxAddress.Parse(mailRequest.Email);
            //email.To.Add(MailboxAddress.Parse(_mailSettings.Mail)); 
            //email.Subject = mailRequest.Subject;
            //var builder = new BodyBuilder();
            //builder.HtmlBody = mailRequest.Body;
            //email.Body = builder.ToMessageBody();
            //using var smtp = new SmtpClient();
            //smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            //smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            //await smtp.SendAsync(email);
            //smtp.Disconnect(true);
            //
            var email = new MimeMessage();

            // Set the Sender and From addresses with customer's name and email
            email.Sender = MailboxAddress.Parse(mailRequest.Email);
            email.From.Add(new MailboxAddress(mailRequest.Name, mailRequest.Email));

            // Add recipient from MailSettings
            email.To.Add(MailboxAddress.Parse(_mailSettings.Mail));

            // Set subject and body
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder
            {
                HtmlBody = $"<p><strong>Name:</strong> {mailRequest.Name}</p>" +
                       $"<p><strong>Email:</strong> {mailRequest.Email}</p>" +
                       $"<p><strong>Subject:</strong> {mailRequest.Subject}</p>" +
                       $"<p><strong>Message:</strong><br>{mailRequest.Body}</p>"
            };
            email.Body = builder.ToMessageBody();
            Console.WriteLine("Email Sender: " + email.Sender);
            Console.WriteLine("Email From: " + email.From);
            Console.WriteLine("Email To: " + email.To);
            // Send the email using SMTP
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
