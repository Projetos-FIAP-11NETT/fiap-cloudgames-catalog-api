using FiapCloudGames.Catalog.Domain.Entities;
using MediatR;

namespace FiapCloudGames.Catalog.Application.CategoryFeature.Queries.GetCategory;

public sealed record class GetCategoryQuery
(
    Guid? Id,
    string? Name
)
    : IRequest<IEnumerable<Category>>;