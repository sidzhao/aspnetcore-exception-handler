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
using Sid.AspNetCore.Exception.Handler.Options;
using Sid.AspNetCore.Exception.Handler.Utils;
using Sid.MailKit.Abstractions;

namespace Sid.AspNetCore.Exception.Handler
{
    public class ClientExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ClientExceptionHandlerMiddleware(RequestDelegate next, ILogger<ClientExceptionHandlerMiddleware> logger, IOptions<ExceptionHandlerOptions> options = null, IErrorSender errorSender = null)
        {
            if (options != null)
            {
                Options = options.Value;

                if (Options.SendErrorEnabled && errorSender == null)
                {
                    throw new ArgumentNullException(nameof(errorSender));
                }
            }

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _next = next;
        }

        public ExceptionHandlerOptions Options { get; set; }

        public ILogger Logger { get; set; }

        public IErrorSender ErrorSender { get; set; }

        public Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (Options.ClientExceptionEnabled && context.Request.Path.Equals(Options.ClientExceptionPath, StringComparison.Ordinal))
            {
                // Request must be POST with Content-Type: application/json
                if (!context.Request.Method.Equals("POST") &&
                    (context.Request.ContentType == null || context.Request.ContentType.Equals("applicatin/json", StringComparison.OrdinalIgnoreCase)))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonResult("Bad Request. Request method must be POST. Request content type must be Application/Json"));
                }

                return Execute(context);
            }

            return _next(context);
        }

        private async Task Execute(HttpContext context)
        {
            string requestBody;
            using (var stremReader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                requestBody = await stremReader.ReadToEndAsync();
            }

            var clientException = JsonConvert.DeserializeObject<ClientExceptionContent>(requestBody);
            var errMessage = BuildMailBody(clientException);

            Logger?.LogError(errMessage);

            if (Options != null && Options.SendErrorEnabled)
            {
                var content = BuildMailBody(clientException);
                await ErrorSender.SendAsync(content);
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        private string BuildMailBody(ClientExceptionContent clientException)
        {
            if (clientException == null)
            {
                return "Didn't get any error message.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Client Exception");
            sb.AppendLine($"Source: {clientException.Source}");
            sb.AppendLine($"Message: {clientException.Message}");

            return sb.ToString();
        }

        private string JsonResult(string message)
        {
            return JsonConvert.SerializeObject(
                new ApiErrorResult
                {
                    Type = ErrorType.System,
                    Message = message
                }
                , Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver =
                        new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                });
        }
    }
}
