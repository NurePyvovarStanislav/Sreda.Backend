using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Common;

namespace Sreda.Backend.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Преобразует Result&lt;T&gt; в HTTP-ответ (200/400/404).</summary>
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Error);
    }

    /// <summary>Преобразует Result в HTTP-ответ (204/400/404).</summary>
    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return MapFailure(result.Error);
    }

    private IActionResult MapFailure(string? error)
    {
        if (!string.IsNullOrEmpty(error) && error.Contains("не найден", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(error);
        }

        return BadRequest(error);
    }
}
