using FiapCloudGames.Catalog.Application.CategoryFeature.Queries.GetCategory;
using FluentValidation;

namespace FiapCloudGames.Catalog.Application.CategoryFeature.Commands.GetCategory;

public sealed class GetCategoryQueryValidator
    : AbstractValidator<GetCategoryQuery>
{
    public GetCategoryQueryValidator()
    {
    }
}