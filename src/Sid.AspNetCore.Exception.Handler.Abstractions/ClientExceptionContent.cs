using System;
using System.Collections.Generic;
using System.Text;

namespace Sid.AspNetCore.Exception.Handler.Abstractions
{
    public class ClientExceptionContent
    {
        public string Source { get; set; }

        public string Message { get; set; }
    }
}
