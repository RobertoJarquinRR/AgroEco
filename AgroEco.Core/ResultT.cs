namespace AgroEco.Core
{
   
    public class Result<T> : Result
    {
        public T? Value { get; }

        private Result(bool success, T? value, string? message, Exception? exception)
            : base(success, message, exception)
        {
            Value = value;
        }

        public static Result<T> CreateSuccess(T value, string? message = null)
        {
            return new Result<T>(true, value, message, null);
        }

        
        public new static Result<T> CreateFailure(string message, Exception? exception = null)
        {
            return new Result<T>(false, default, message, exception);
        }
    }
}