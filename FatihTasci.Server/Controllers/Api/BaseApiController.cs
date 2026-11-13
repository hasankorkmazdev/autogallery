using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FatihTasci.Server.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
}
