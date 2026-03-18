using FiapCloudGames.Catalog.Application.GameFeature.Commands.CreateGame;
using FiapCloudGames.Catalog.Application.GameFeature.Commands.DeleteGame;
using FiapCloudGames.Catalog.Application.GameFeature.Commands.UpdateGame;
using FiapCloudGames.Catalog.Application.GameFeature.Queries.GetGame;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Catalog.Api.Controllers;

[Route("api/v1/[controller]")]
public class GameController
(
    IMediator mediator
)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetAsync([FromQuery] GetGameQuery query)
    {
        var result = await mediator.Send(query);
    
        if(result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateGameCommand command)
    {
        var result = await mediator.Send(command);

        if (result)
            return Created();

        return BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateGameCommand command)
    {
        var result = await mediator.Send(command);
    
        if (result)
            return Ok(result);
    
        return BadRequest();
    }
    
    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync([FromQuery] DeleteGameCommand query)
    {
        var result = await mediator.Send(query);
    
        if (result)
            return Ok(result);
    
        return BadRequest();
    }
}
