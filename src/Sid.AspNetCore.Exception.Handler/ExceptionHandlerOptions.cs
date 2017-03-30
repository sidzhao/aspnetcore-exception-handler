using System;

namespace Sid.AspNetCore.Exception.Handler
{
    public class ExceptionHandlerOptions
    {
        public MailOptions MailOptions { get; set; }

        public Action<System.Exception> ManualProcess { get; set; }

        public bool OutputErrorResult { get; set; } = true;

        public bool JsExceptionEnabled { get; set; }

        public string JsExceptionPath = "/exception/js";
    }
}