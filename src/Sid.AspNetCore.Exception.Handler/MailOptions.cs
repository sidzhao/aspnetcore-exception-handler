using System.Collections.Generic;
using Sid.MailKit.Abstractions;

namespace Sid.AspNetCore.Exception.Handler
{
    public class MailOptions
    {
        public string Subject { get; set; }

        public IEnumerable<MailAddress> Tos { get; set; }
    }
}
