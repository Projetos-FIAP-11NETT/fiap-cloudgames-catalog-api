using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.CategoryFeature.Commands.DeleteCategory;

public sealed record class DeleteCategoryCommand
(
    Guid Id
)
    : IRequest<bool>;