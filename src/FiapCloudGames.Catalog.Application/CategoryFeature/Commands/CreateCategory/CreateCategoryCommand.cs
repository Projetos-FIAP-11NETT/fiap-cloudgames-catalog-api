using MediatR;

namespace FiapCloudGames.Catalog.Application.CategoryFeature.Commands.CreateCategory;

public sealed record class CreateCategoryCommand
(
    string Name
)
    : IRequest<bool>;