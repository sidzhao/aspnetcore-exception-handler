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
using Sid.MailKit.Abstractions;
using Sid.AspNetCore.Exception.Handler;
using Sid.AspNetCore.Exception.Handler.Options;
using Xunit;

namespace Sid.AspNetCore.Exception.Handler.Tests
{
    public class ExceptionHandlerMiddlewareTest
    {
        [Fact]
        public async Task TestExceptionLogger()
        {
            Mock<IMailSender> emailSenderMock = null;
            int processManualTimes = 0;

            var hostBuilder = new WebHostBuilder();
            hostBuilder.ConfigureServices(collection =>
            {
                emailSenderMock = new Mock<IMailSender>();
                emailSenderMock.Setup(
                    p => p.SendEmailAsync(It.IsAny<MailMessage>()));
                collection.AddSingleton<IMailSender>(provider => emailSenderMock.Object);

                collection.AddSidExceptionHandler(options =>
                {
                    options.Tos = new List<MailAddress>
                    {
                        new MailAddress {Address = "123@test.com"},
                        new MailAddress {Address = "246@test.com"}
                    };
                    options.Subject = "Test Error";
                }
                //, provider =>
                //{
                //    var sender = new Mock<IMailSender>();
                //    sender.Setup(
                //        p => p.SendEmailAsync(It.IsAny<MailMessage>()));
                //    return sender.Object;
                //}
                );
            });
            hostBuilder.Configure(app =>
            {
                var options = new ExceptionHandlerOptions
                {
                    SendErrorEnabled = true,
                    ManualProcess = exception => { processManualTimes++; }
                };


                app.UseSidExceptionHandler(options);
                app.Run(context =>
                {
                    throw new System.Exception("Server Error");
                });
            });

            using (var testServer = new TestServer(hostBuilder))
            {
                var response = await testServer.CreateRequest("/").GetAsync();
                Assert.Equal(500, (int)response.StatusCode);
                var result = JsonConvert.DeserializeObject<ApiErrorResult>(await response.Content.ReadAsStringAsync());
                Assert.NotNull(result);
                Assert.Equal("Server Error", result.Message);

                Assert.Equal(1, processManualTimes);

                emailSenderMock.Verify(
                        p => p.SendEmailAsync(It.IsAny<MailMessage>()), Times.Once);
            }
        }
    }
}
