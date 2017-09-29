using System;

namespace Sid.AspNetCore.Exception.Handler.Options
{
    public class ExceptionHandlerOptions
    {
        public Action<System.Exception> ManualProcess { get; set; }

        public bool OutputErrorResult { get; set; } = true;

        public bool ClientExceptionEnabled { get; set; }

        /// <summary>
        /// Default path is /exception/client
        /// </summary>
        public string ClientExceptionPath = "/exception/client";

        public bool SendErrorEnabled { get; set; } = false;

        public bool IsProduction { get; set; } = false;

        public string ProductionErrorMessage { get; set; } = "Server error, please contact to administrator.";
    }
}