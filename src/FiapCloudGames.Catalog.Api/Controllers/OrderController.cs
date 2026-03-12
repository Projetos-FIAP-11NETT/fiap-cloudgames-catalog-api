using System.Security.Claims;
using FiapCloudGames.Catalog.Application.OrderFeature.Commands.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Catalog.Api.Controllers;

[Route("api/v1/[controller]")]
public class OrderController
(
    IMediator mediator
)
    : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirstValue("user_id");
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        var name = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("name");

        var command = new CreateOrderCommand(userId!, email!, name!, request.GameId);
        var result = await mediator.Send(command);

        if (result)
            return Created();

        return BadRequest();
    }
}

public record CreateOrderRequest(Guid GameId);