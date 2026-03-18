using FluentValidation;

namespace FiapCloudGames.Catalog.Application.CategoryFeature.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandValidator
    : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O Guid não pode ser vazio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome não pode ser vazio.");
    }
}