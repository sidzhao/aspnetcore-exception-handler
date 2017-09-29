using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Sid.AspNetCore.Exception.Handler.Abstractions;
using Sid.AspNetCore.Exception.Handler.Options;
using Sid.MailKit.Abstractions;
using Xunit;

namespace Sid.AspNetCore.Exception.Handler.Tests
{
    public class ClientExceptionHandlerMiddlewareTest
    {
        [Fact]
        public async Task TestExceptionLogger()
        {
            var hostBuilder = new WebHostBuilder();
            hostBuilder
                .ConfigureServices(collection =>
                {
                    collection.AddSidExceptionHandler();
                })
                .Configure(app =>
            {
                var options = new ExceptionHandlerOptions { ClientExceptionEnabled = true };

                app.UseSidExceptionHandler(options);
            });

            using (var testServer = new TestServer(hostBuilder))
            {
                var response = await testServer.CreateRequest("/exception/client").GetAsync();
                Assert.Equal(400, (int)response.StatusCode);

                response = await testServer.CreateRequest("/exception/client").PostAsync();
                Assert.True(response.IsSuccessStatusCode);
            }
        }
    }
}
