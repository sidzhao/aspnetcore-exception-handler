using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sid.AspNetCore.Exception.Handler.Abstractions;
using Sid.MailKit.Abstractions;

namespace Sid.AspNetCore.Exception.Handler
{
    public class JsExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        
        public JsExceptionHandlerMiddleware(RequestDelegate next, ILogger<JsExceptionHandlerMiddleware> logger, IOptions<ExceptionHandlerOptions> options = null, IMailSender mailSender = null)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (options != null)
            {
                Options = options.Value;
                if (Options.MailOptions != null)
                {
                    if (Options.MailOptions.Tos == null)
                    {
                        throw new ArgumentNullException(nameof(Options.MailOptions.Tos));
                    }

                    if (!Options.MailOptions.Tos.Any())
                    {
                        throw new System.Exception("At lease has one email to address.");
                    }

                    if (string.IsNullOrEmpty(Options.MailOptions.Subject))
                    {
                        throw new ArgumentNullException(nameof(Options.MailOptions.Subject));
                    }

                    if (mailSender == null)
                    {
                        throw new ArgumentNullException(nameof(mailSender));
                    }

                    MailSender = mailSender;
                }
            }

            Logger = logger;
            _next = next;
        }

        public ExceptionHandlerOptions Options { get; set; }

        public ILogger Logger { get; set; }

        public IMailSender MailSender { get; set; }

        public async Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (Options.JsExceptionEnabled && !context.Request.Path.Equals(Options.JsExceptionPath, StringComparison.Ordinal))
            {
                // Request must be POST with Content-Type: application/json
                if (!context.Request.Method.Equals("POST") && context.Request.ContentType.Equals("applicatin/json", StringComparison.OrdinalIgnoreCase))
                {
                    throw new System.Exception("Bad Request. Request method must be POST. Request content type must be Application/Json");
                }

                await Execute(context);
            }

            await _next(context);
        }

        private async Task Execute(HttpContext context)
        {
            string requestBody;
            using (var stremReader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                requestBody = await stremReader.ReadToEndAsync();
            }

            var jsException = JsonConvert.DeserializeObject<JsExceptionModel>(requestBody);
            var errMessage = BuildMailBody(jsException);

            Logger.LogInformation(errMessage);

            await MailSender.SendEmailAsync(new MailMessage
            {
                Subject = Options.MailOptions.Subject,
                Content = BuildMailBody(jsException),
                Tos = Options.MailOptions.Tos.ToList()
            });
        }

        private string BuildMailBody(JsExceptionModel jsException)
        {
            var sb = new StringBuilder();
            sb.AppendLine("JS Exception");
            sb.AppendLine($"Url: {jsException.Url}");
            sb.AppendLine($"Message: {jsException.Message}");
            sb.AppendLine($"Stack: {jsException.Stack}");

            return sb.ToString();
        }
    }
}
