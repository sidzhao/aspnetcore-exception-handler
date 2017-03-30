namespace Sid.AspNetCore.Exception.Handler.Abstractions
{
    public class ApiErrorResult
    {
        public ApiErrorResult()
        {
        }

        public ApiErrorResult(ErrorType type, int code, string message)
        {
            Type = type;
            Code = code;
            Message = message;
        }

        public ErrorType Type { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }
    }
}
