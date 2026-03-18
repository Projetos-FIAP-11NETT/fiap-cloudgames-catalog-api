using FluentValidation;

namespace FiapCloudGames.Catalog.Application.CategoryFeature.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O Nome da categoria não pode estar vazio.");
    }
}