
namespace AgroEco.Core
{
    public class Result
    {
        
        public bool Success {  get; private set; }

        public string? Message { get; private set; }

        public Exception? Exception { get; private set; }

        protected Result( bool success, string? message, Exception? e)
        {
            Success = success;
            Message = message;
            Exception = e; 
        }
       
        public static Result CreateSuccess(string? mensaje = null)
        {
            return new Result(true, mensaje,null);
        }       

        public static Result CreateFailure(string? mensaje, Exception? exception = null)
        {
            return new Result(false, mensaje, exception);
        }

    }
}
