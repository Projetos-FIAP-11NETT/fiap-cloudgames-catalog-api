using FiapCloudGames.Catalog.Application.Features.LibraryItemFeature.Queries.GetLibraryItemsByUser;
using FiapCloudGames.Catalog.Domain.Contracts.Publishers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FiapCloudGames.Catalog.Api.Controllers;

[Route("api/v1/[controller]")]
public class LibraryItemController
(
    IMediator mediator,
    IEmailNotificationPublisher emailNotificationPublisher
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

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmailNotificationAsync(
        [FromBody] TestEmailRequest request,
        CancellationToken cancellationToken)
    {
        await emailNotificationPublisher.PublishAsync(
            request.To,
            request.Subject,
            request.Body,
            cancellationToken);

        return Ok(new { message = "Mensagem publicada na fila com sucesso." });
    }

    public record TestEmailRequest(string To, string Subject, string Body);
}