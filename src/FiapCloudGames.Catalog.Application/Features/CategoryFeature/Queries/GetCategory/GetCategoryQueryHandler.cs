using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Entities;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.CategoryFeature.Queries.GetCategory;

public class GetCategoryQueryHandler
    (
        ICategoryRepository categoryRepository
    )
    : IRequestHandler<GetCategoryQuery,IEnumerable<GetCategoryResponse>>
{

    public async Task<IEnumerable<GetCategoryResponse>> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        var categorias = await categoryRepository.GetCategory(query.Id, query.Name);
        return categorias.Select(x => new GetCategoryResponse
        {
            Id = x.Id,
            Name = x.Name,
        });
    }
}