using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Sid.AspNetCore.Exception.Handler.Abstractions;

namespace Sid.AspNetCore.Exception.Handler.Utils
{
    public class ErrorContentCreator : IErrorContentCreator
    {
        public string BuildContent(System.Exception ex, HttpContext context = null)
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

            if (context.Request.Body.CanSeek)
            {
                context.Request.Body.Position = 0;
                string bodyString;
                using (var stremReader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    context.Request.Body.Position = 0;
                    bodyString = stremReader.ReadToEnd();
                }
                sb.AppendLine($"Request Body: {bodyString}");
            }
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
