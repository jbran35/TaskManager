using System.Net.NetworkInformation;

namespace TaskManager.Domain.Common
{
    public class Result 
    {
        public bool IsSuccess { get; }

        public string? SuccessMessage { get; }
        public string? ErrorMessage { get; }

        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, string? successMessage, string? errorMessage)
        {
            IsSuccess = isSuccess;
            SuccessMessage = successMessage;
            ErrorMessage = errorMessage;
        }
        
        
        public static Result Success(string successMessage = "") => new Result (true, successMessage, null);
        public static Result Failure(string errorMessage) => new Result(false, null, errorMessage);
    }


    public class Result<T> : Result
    {
        private readonly T? _value;
        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result can't be accessed.");

        protected Result(T? value, bool isSuccess, string success, string error)
            : base(isSuccess, success, error)
        {
            _value = value;
        }

        public static Result<T> Success(T value, string success="") => new(value, true, success, string.Empty);
        public new static Result<T> Failure(string error) => new(default, false, string.Empty, error);

    }
}
