namespace BookSpace.Core.Common;

public enum ErrorCode
{
    None = 0,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unauthorized,
    LimitExceeded
}
