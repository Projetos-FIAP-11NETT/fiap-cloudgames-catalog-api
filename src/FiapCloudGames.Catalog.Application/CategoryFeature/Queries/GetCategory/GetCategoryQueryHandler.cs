using FiapCloudGames.Catalog.Application.CategoryFeature.Queries.GetCategory;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Entities;
using MediatR;

namespace FiapCloudGames.Catalog.Application.CategoryFeature.Commands.GetCategory;

public class GetCategoryQueryHandler
    (
        ICategoryRepository categoryRepository
    )
    : IRequestHandler<GetCategoryQuery,IEnumerable<Category>>
{

    public async Task<IEnumerable<Category>> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        return await categoryRepository.GetCategory(query.Id, query.Name);
    }
}