using FluentValidation;

namespace FiapCloudGames.Catalog.Application.Features.CategoryFeature.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandValidator
    : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O Guid não pode ser vazia.");
    }
}