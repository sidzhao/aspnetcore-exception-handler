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
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IErrorContentCreator errorContentCreator, IOptions<ExceptionHandlerOptions> options = null, IErrorSender errorSender = null)
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
            ErrorContentCreator = errorContentCreator ?? throw new ArgumentNullException(nameof(errorContentCreator));
            ErrorSender = errorSender;

            _next = next;
        }

        public ExceptionHandlerOptions Options { get; }

        public ILogger Logger { get; }

        public IErrorContentCreator ErrorContentCreator { get; }

        public IErrorSender ErrorSender { get; }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (System.Exception ex)
            {
                // If it's NonSystemException, no need to send error email
                if (!(ex is NonSystemException))
                {
                    Logger.LogError(0, ex, "An unhandled exception has occurred: " + ex.Message);

                    var errMessage = ErrorContentCreator.BuildContent(ex, context);
                    Logger.LogError(errMessage);

                    Options?.ManualProcess?.Invoke(ex);

                    if (Options != null && Options.SendErrorEnabled)
                    {
                        try
                        {
                            var content = ErrorContentCreator.BuildContent(ex, context);
                            await ErrorSender.SendAsync(content);
                        }
                        catch (System.Exception ex2)
                        {
                            Logger.LogError(0, ex2, "An unhandled exception has occurred during send error email: " + ex2.Message);
                            Logger.LogError(ErrorContentCreator.BuildContent(ex2));
                        }
                    }
                }

                // Output error result
                if (Options != null && Options.OutputErrorResult)
                {
                    var response = new ApiErrorResult();
                    var nonSystemException = ex as NonSystemException;
                    if (nonSystemException != null)
                    {
                        response.Type = ErrorType.NonSystem;
                        response.Code = nonSystemException.ErrorCode;
                        response.Message = nonSystemException.Message;
                    }
                    else
                    {
                        response.Type = ErrorType.System;
                        response.Message = Options.IsProduction ? Options.ProductionErrorMessage : ex.Message;
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
    }
}
