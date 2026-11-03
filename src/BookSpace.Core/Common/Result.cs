namespace BookSpace.Core.Common;

public class Result
{
    protected Result(bool isSuccess, ErrorCode error, string message)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
    }

    public bool IsSuccess { get; }
    public ErrorCode Error { get; }
    public string Message { get; }

    public static Result Ok() => new(true, ErrorCode.None, string.Empty);
    public static Result Fail(ErrorCode error, string message) => new(false, error, message);
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, ErrorCode error, string message)
        : base(isSuccess, error, message)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Ok(T value) => new(true, value, ErrorCode.None, string.Empty);
    public static new Result<T> Fail(ErrorCode error, string message) => new(false, default, error, message);
}
