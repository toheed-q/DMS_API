namespace DMS_Backend.Common
{
    /// <summary>Category of a failure, mapped to an HTTP status by the base controller.</summary>
    public enum ErrorType
    {
        None,
        Validation,
        Unauthorized,
        Forbidden,
        NotFound,
        Conflict,
        Internal
    }

    /// <summary>
    /// A lightweight success/failure envelope so services can report outcomes
    /// without throwing exceptions for expected cases. Keeps controllers thin and
    /// error→HTTP mapping in one place (DRY).
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? Error { get; }
        public ErrorType ErrorType { get; }

        private Result(bool isSuccess, T? value, string? error, ErrorType errorType)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            ErrorType = errorType;
        }

        public static Result<T> Success(T value) => new(true, value, null, ErrorType.None);

        public static Result<T> Failure(string error, ErrorType errorType) =>
            new(false, default, error, errorType);
    }
}
