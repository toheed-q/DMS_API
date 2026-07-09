using DMS_Backend.Common;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    /// <summary>
    /// Base for all API controllers. Centralizes the Result -> HTTP mapping so
    /// every endpoint stays a one-liner and responds consistently (DRY).
    /// </summary>
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Value);

            var payload = new { message = result.Error };
            return result.ErrorType switch
            {
                ErrorType.Validation   => BadRequest(payload),
                ErrorType.Unauthorized => Unauthorized(payload),
                ErrorType.Forbidden    => StatusCode(StatusCodes.Status403Forbidden, payload),
                ErrorType.NotFound     => NotFound(payload),
                ErrorType.Conflict     => Conflict(payload),
                _                      => StatusCode(StatusCodes.Status500InternalServerError, payload)
            };
        }
    }
}
