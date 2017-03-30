using System;
using System.IO;
using System.Linq;
using System.Net;
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
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        
        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IOptions<ExceptionHandlerOptions> options = null, IMailSender mailSender = null)
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
            try
            {
                await _next.Invoke(context);
            }
            catch (System.Exception ex)
            {
                Logger.LogError(0, ex, "An unhandled exception has occurred: " + ex.Message);

                var errMessage = BuildErrorMessage(ex, context);
                Logger.LogError(errMessage);

                Options.ManualProcess?.Invoke(ex);

                if (Options?.MailOptions != null && MailSender != null)
                {
                    try
                    {
                        await MailSender.SendEmailAsync(new MailMessage
                        {
                            Subject = Options.MailOptions.Subject,
                            Content = errMessage,
                            Tos = Options.MailOptions.Tos.ToList()
                        });
                    }
                    catch (System.Exception ex2)
                    {
                        Logger.LogError(0, ex2, "An unhandled exception has occurred during send error email: " + ex2.Message);
                        Logger.LogError(BuildErrorMessage(ex2));
                    }
                }

                // Output error result
                if (Options != null && Options.OutputErrorResult)
                {
                    var response = new ApiErrorResult();
                    var exception = ex as NonSystemException;
                    if (exception != null)
                    {
                        response.Type = ErrorType.NonSystem;
                        response.Code = exception.ErrorCode;
                        response.Message = exception.Message;
                    }
                    else
                    {
                        response.Type = ErrorType.System;
                        response.Message = ex.Message;
                    }

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ContractResolver =
                                new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                        }));
                }
            }
        }

        private string BuildErrorMessage(System.Exception ex, HttpContext context = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("-------------------- Exception Details --------------------");
            GetErrorMessage(ex, sb);
            if (context != null)
            {
                sb.AppendLine("-------------------- Request Infomation --------------------");
                GetRequestInfo(context, sb);
            }
            return sb.ToString();
        }

        private void GetRequestInfo(HttpContext context, StringBuilder sb)
        {
            sb.AppendLine($"Request Head: {JsonConvert.SerializeObject(context.Request.Headers)}");
            sb.AppendLine($"Request Host: {context.Request.Host}");
            sb.AppendLine($"Request Path: {context.Request.Path}");
            sb.AppendLine($"Request Query String: {context.Request.QueryString}");

            string bodyString;
            using (var stremReader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                bodyString = stremReader.ReadToEnd();
            }
            sb.AppendLine($"Request Body: {bodyString}");
        }

        private void GetErrorMessage(System.Exception ex, StringBuilder sb)
        {
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Source: {ex.Source}");
            sb.AppendLine($"StackTrace:");
            sb.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                sb.AppendLine("-------------------- InnertException --------------------");
                GetErrorMessage(ex.InnerException, sb);
            }
        }
    }
}
