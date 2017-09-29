using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sid.AspNetCore.Exception.Handler.Abstractions;
using Sid.AspNetCore.Exception.Handler.Options;
using Sid.MailKit.Abstractions;

namespace Sid.AspNetCore.Exception.Handler.Utils
{
    public class MailErrorSender : IErrorSender
    {
        private readonly IMailSender _mailSender;
        private readonly MailOptions _options;

        public MailErrorSender(IOptions<MailOptions> options, IMailSender mailSender)
        {
            _mailSender = mailSender ?? throw new ArgumentNullException(nameof(mailSender));

            if (options == null) throw new ArgumentNullException(nameof(options));
            _options = options.Value;

            if(string.IsNullOrEmpty(_options.Subject))throw new ArgumentNullException(nameof(_options.Subject));
            if (_options.Tos == null) throw new ArgumentNullException(nameof(_options.Tos));
        }

        public void Send(string content)
        {
            _mailSender.SendEmail(new MailMessage(_options.Subject, content, _options.Tos.ToList()));
        }

        public async Task SendAsync(string content)
        {
            await _mailSender.SendEmailAsync(new MailMessage(_options.Subject, content, _options.Tos.ToList()));
        }
    }
}
