using System.Security.Claims;
using FiapCloudGames.Catalog.Application.LibraryItemFeature.Queries.GetLibraryItemsByUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Catalog.Api.Controllers;

[Route("api/v1/[controller]")]
public class LibraryItemController
(
    IMediator mediator
)
    : ControllerBase
{
    [HttpGet("my")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetMyLibraryAsync()
    {
        var userIdString = User.FindFirstValue("system_user_id");
        var userId = Guid.Parse(userIdString!);
        var result = await mediator.Send(new GetLibraryItemsByUserQuery(userId));
        return Ok(result);
    }
}