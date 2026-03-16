using FiapCloudGames.Catalog.Application.CategoryFeature.Commands.CreateCategory;
using FiapCloudGames.Catalog.Application.CategoryFeature.Commands.DeleteCategory;
using FiapCloudGames.Catalog.Application.CategoryFeature.Commands.UpdateCategory;
using FiapCloudGames.Catalog.Application.CategoryFeature.Queries.GetCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Catalog.Api.Controllers;

[Route("api/v1/[controller]")]
public class CategoryController
(
    IMediator mediator
)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetAsync([FromQuery] GetCategoryQuery query)
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
    public async Task<IActionResult> CreateAsync([FromBody] CreateCategoryCommand command)
    {
        var result = await mediator.Send(command);

        if (result)
            return Created();

        return BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateCategoryCommand command)
    {
        var result = await mediator.Send(command);
    
        if (result)
            return Ok(result);
    
        return BadRequest();
    }
    
    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync([FromQuery] DeleteCategoryCommand query)
    {
        var result = await mediator.Send(query);
    
        if (result)
            return Ok(result);
    
        return BadRequest();
    }
}