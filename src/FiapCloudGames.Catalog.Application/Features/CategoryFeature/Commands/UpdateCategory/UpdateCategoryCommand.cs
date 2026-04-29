using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.CategoryFeature.Commands.UpdateCategory;

public sealed record class UpdateCategoryCommand
(
    Guid Id,
    string Name
)
    : IRequest<bool>;