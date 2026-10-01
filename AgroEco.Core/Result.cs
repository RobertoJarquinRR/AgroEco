
namespace AgroEco.Core
{
    public class Result
    {
        
        public virtual bool Success { get; private set; }

        public string? Message { get; private set; }

        public Exception? Exception { get; private set; }

        protected Result(bool success, string? message, Exception? exception)
        {
            Success = success;
            Message = message;
            Exception = exception;
        }
       
        public static Result CreateSuccess(string? message = null)
        {
            return new Result(true, message, null);
        }       

        public static Result CreateFailure(string? message, Exception? exception = null)
        {
            return new Result(false, message, exception);
        }

    }
}
