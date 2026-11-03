using BookSpace.Core.Common;
using BookSpace.Web.Configuration;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookSpace.Web.Controllers.Api;

/// <summary>REST API-ს საერთო ბაზა: JSON, rate limiting, ბიზნეს-შეცდომის HTTP სტატუსზე ასახვა.</summary>
[ApiController]
[IgnoreAntiforgeryToken]
[EnableRateLimiting(ApiRateLimitOptions.PolicyName)]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorDto), StatusCodes.Status429TooManyRequests)]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ErrorResult(Result result)
    {
        var status = result.Error switch
        {
            ErrorCode.Validation => StatusCodes.Status400BadRequest,
            ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCode.Forbidden => StatusCodes.Status403Forbidden,
            ErrorCode.NotFound => StatusCodes.Status404NotFound,
            ErrorCode.Conflict => StatusCodes.Status409Conflict,
            ErrorCode.LimitExceeded => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
        return StatusCode(status, new ErrorDto(result.Error.ToString(), result.Message));
    }
}
