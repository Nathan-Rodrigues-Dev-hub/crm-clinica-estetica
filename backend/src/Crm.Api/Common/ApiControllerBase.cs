using Crm.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToResponse<T>(Result<T> result, int successStatus = StatusCodes.Status200OK)
        => this.ToHttp(result, successStatus);

    protected IActionResult ToResponse(Result result, int successStatus = StatusCodes.Status200OK)
        => this.ToHttp(result, successStatus);
}
