using System;
using System.Collections.Generic;
using System.Text;

namespace Sid.AspNetCore.Exception.Handler.Abstractions
{
    public class JsExceptionModel
    {
        public string Url { get; set; }

        public string Message { get; set; }

        public string Stack { get; set; }
    }
}
