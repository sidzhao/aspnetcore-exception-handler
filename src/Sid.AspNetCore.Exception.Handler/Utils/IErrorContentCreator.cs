using Microsoft.AspNetCore.Http;

namespace Sid.AspNetCore.Exception.Handler.Utils
{
    public interface IErrorContentCreator
    {
        string BuildContent(System.Exception ex, HttpContext context = null);
    }
}
