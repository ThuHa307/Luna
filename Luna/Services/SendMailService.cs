using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

public class MailSetting {
    public string? Mail {set; get;}
    public string? Password {set; get;}
    public string? DisplayName {set; get;}
    public string? Host {set; get;}
    public int Port {set; get;}
}
public class SendMailService : IEmailSender {

    public MailSetting _mailSetting {set; get;}
    private readonly ILogger<SendMailService> _logger;
    public SendMailService(IOptions<MailSetting> mailSetting, ILogger<SendMailService> logger) {
        _mailSetting = mailSetting.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.Sender = new MailboxAddress(_mailSetting.DisplayName, _mailSetting.Mail);
        message.From.Add(new MailboxAddress(_mailSetting.DisplayName, _mailSetting.Mail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;

        var builder = new BodyBuilder();
        builder.HtmlBody = htmlMessage;
        message.Body = builder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();

        try {
            smtp.Connect (_mailSetting.Host, _mailSetting.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate (_mailSetting.Mail, _mailSetting.Password);
            await smtp.SendAsync(message);
        }
        catch (Exception ex) {
            // Gửi mail thất bại, nội dung email sẽ lưu vào thư mục mailssave
            System.IO.Directory.CreateDirectory("mailssave");
            var emailsavefile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
            await message.WriteToAsync(emailsavefile);

            _logger.LogInformation("Lỗi gửi mail, lưu tại - " + emailsavefile);
        }

        smtp.Disconnect (true);

        _logger.LogInformation("send mail to " + email);
    }
}

public class MailContent {
    public string? To {set; get;}
    public string? Subject {get; set;}
    public string? Body {set;get;}
}