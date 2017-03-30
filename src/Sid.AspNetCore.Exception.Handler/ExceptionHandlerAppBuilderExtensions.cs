using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Sid.MailKit.Abstractions;

namespace Sid.AspNetCore.Exception.Handler
{
    public static class ExceptionHandlerAppBuilderExtensions
    {
        public static IApplicationBuilder UseSidExceptionHandler(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<ExceptionHandlerMiddleware>();
        }

        public static IApplicationBuilder UseSidExceptionHandler(this IApplicationBuilder app, IList<MailAddress> tos, string subject)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new ExceptionHandlerOptions { MailOptions = new MailOptions { Subject = subject, Tos = tos } };

            return UseSidExceptionHandler(app, options);
        }

        public static IApplicationBuilder UseSidExceptionHandler(this IApplicationBuilder app, Action<System.Exception> manualProcess)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new ExceptionHandlerOptions { ManualProcess = manualProcess };

            return UseSidExceptionHandler(app, options);
        }

        public static IApplicationBuilder UseSidExceptionHandler(this IApplicationBuilder app, ExceptionHandlerOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var optionsWrapper = Options.Create(options);
            app.UseMiddleware<ExceptionHandlerMiddleware>(optionsWrapper);

            if (options.JsExceptionEnabled)
            {
                app.UseMiddleware<JsExceptionHandlerMiddleware>(optionsWrapper);
            }

            return app;
        }
    }
}
