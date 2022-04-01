using Domains;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppApi.Bl
{
    public interface IMailService
    {
        Task SendEmailAsync(string mailTo, string subject, string body);
    }


    public class SendMailService : IMailService
    {

        private readonly MailSettingsViewModel _mailSettingsViewModel;
        public SendMailService(IOptions<MailSettingsViewModel> mailSettingsViewModel)
        {
            _mailSettingsViewModel = mailSettingsViewModel.Value;
        }

        // Function Send Email
        public async Task SendEmailAsync(string mailTo, string subject, string body)
        {
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_mailSettingsViewModel.Email),
                Subject = subject
            };

            email.To.Add(MailboxAddress.Parse(mailTo));

            var builder = new BodyBuilder();
            builder.HtmlBody = body;

            email.Body = builder.ToMessageBody();
            email.From.Add(new MailboxAddress(_mailSettingsViewModel.DisplayName, _mailSettingsViewModel.Email));

            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettingsViewModel.Host, _mailSettingsViewModel.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettingsViewModel.Email, _mailSettingsViewModel.Password);

            await smtp.SendAsync(email);
            smtp.Disconnect(true);

        }

    }

}
