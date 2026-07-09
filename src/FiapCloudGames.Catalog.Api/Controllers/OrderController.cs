using FiapCloudGames.Catalog.Application.Features.OrderFeature.Commands.CreateOrder;
using FiapCloudGames.Catalog.Application.Features.OrderFeature.Queries.GetAllOrders;
using FiapCloudGames.Catalog.Application.Features.OrderFeature.Queries.GetMyOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FiapCloudGames.Catalog.Api.Controllers;

[Route("api/v1/[controller]")]
public class OrderController
(
    IMediator mediator
)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await mediator.Send(new GetAllOrdersQuery());
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetMyOrdersAsync()
    {
        var userIdString = User.FindFirstValue("system_user_id");
        var userId = Guid.Parse(userIdString!);
        var result = await mediator.Send(new GetMyOrdersQuery(userId!));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateOrderRequest request)
    {
        var userIdString = User.FindFirstValue("system_user_id");

        if (string.IsNullOrWhiteSpace(userIdString))
            return BadRequest("User ID faltando.");

        var userId = Guid.Parse(userIdString!);
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