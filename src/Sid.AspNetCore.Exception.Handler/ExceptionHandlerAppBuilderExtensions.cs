using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Sid.MailKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Sid.AspNetCore.Exception.Handler.Abstractions;
using Sid.AspNetCore.Exception.Handler.Options;
using Sid.AspNetCore.Exception.Handler.Utils;

namespace Sid.AspNetCore.Exception.Handler
{
    public static class ExceptionHandlerAppBuilderExtensions
    {
        public static void AddSidExceptionHandler(this IServiceCollection services, Action<MailOptions> configureOptions, Func<IServiceProvider, IMailSender> implementationFactory = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddOptions();
            services.Configure(configureOptions);

            if (implementationFactory != null)
            {
                services.AddSingleton<IMailSender>(implementationFactory);
            }

            services.AddSingleton<IErrorContentCreator, ErrorContentCreator>();
            services.AddSingleton<IErrorSender, MailErrorSender>();

            services.AddSidExceptionHandler();
        }

        public static void AddSidExceptionHandler(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IErrorContentCreator, ErrorContentCreator>();
        }

        public static IApplicationBuilder UseSidExceptionHandler(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<ExceptionHandlerMiddleware>();
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

            var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
            app.UseMiddleware<ExceptionHandlerMiddleware>(optionsWrapper);

            if (options.ClientExceptionEnabled)
            {
                app.UseMiddleware<ClientExceptionHandlerMiddleware>(optionsWrapper);
            }

            return app;
        }
    }
}
