using System;

namespace Sid.AspNetCore.Exception.Handler
{
    public class ExceptionHandlerOptions
    {
        public MailOptions MailOptions { get; set; }

        public Action<System.Exception> ManualProcess { get; set; }

        public bool OutputErrorResult { get; set; } = true;

        public bool ClientExceptionEnabled { get; set; }

        public string ClientExceptionPath = "/exception/client";
    }
}