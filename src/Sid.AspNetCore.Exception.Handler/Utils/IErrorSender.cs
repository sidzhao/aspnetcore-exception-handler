using System.Threading.Tasks;

namespace Sid.AspNetCore.Exception.Handler.Utils
{
    public interface IErrorSender
    {
        void Send(string content);

        Task SendAsync(string content);
    }
}
