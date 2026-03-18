using System;
using System.Linq;
using FluentValidation;

namespace FiapCloudGames.Catalog.Application.GameFeature.Commands.CreateGame;

public sealed class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título não pode estar vazio")
            .MaximumLength(80).WithMessage("O título deve ter no máximo 80 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição é obrigatória")
            .MinimumLength(10).WithMessage("A descrição deve ter no mínimo 10 caracteres.")
            .MaximumLength(500).WithMessage("A descrição deve conter no máximo 500 caracteres.");

        RuleFor(x => x.Developer)
            .NotEmpty().WithMessage("O desenvolvedor é obrigatório.")
            .MaximumLength(80).WithMessage("O desenvolvedor deve ter no máximo 80 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Não é permitido valores negativos para o preço.");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty().WithMessage("Data de lançamento inválida.")
            .Must(date => date.Year >= 1950).WithMessage("Data de lançamento inválida.")
            .Must(date => date <= DateTime.UtcNow).WithMessage("A data de lançamento não pode ser no futuro.");

        RuleFor(x => x.Categories)
            .NotEmpty().WithMessage("O jogo deve ter pelo menos uma categoria.")
            .Must(c => c != null && c.Distinct().Count() == c.Count).WithMessage("Categoria duplicada não é permitida.");
    }
}