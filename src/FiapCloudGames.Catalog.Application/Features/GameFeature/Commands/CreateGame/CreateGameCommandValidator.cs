using System;
using System.Linq;
using FluentValidation;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Commands.CreateGame;

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

        RuleFor(x => x.Metadata).NotNull().WithMessage("Os metadados do jogo são obrigatórios.");

        RuleFor(x => x.Metadata.Platforms)
            .NotNull()
            .Must(x => x.Count > 0).WithMessage("Informe pelo menos uma plataforma.")
            .Must(x => x.Distinct(StringComparer.OrdinalIgnoreCase).Count() == x.Count)
            .WithMessage("Plataformas duplicadas não são permitidas.");

        RuleForEach(x => x.Metadata.Platforms)
            .NotEmpty().WithMessage("Plataforma inválida.")
            .MaximumLength(40).WithMessage("A plataforma deve ter no máximo 40 caracteres.");

        RuleForEach(x => x.Metadata.Tags)
            .NotEmpty().WithMessage("Tag inválida.")
            .MaximumLength(40).WithMessage("A tag deve ter no máximo 40 caracteres.");

        RuleFor(x => x.Metadata.AgeRating)
            .NotEmpty().WithMessage("A classificação indicativa é obrigatória.")
            .MaximumLength(10).WithMessage("A classificação indicativa deve ter no máximo 10 caracteres.");

        RuleForEach(x => x.Metadata.Languages)
            .NotEmpty().WithMessage("Idioma inválido.")
            .MaximumLength(20).WithMessage("O idioma deve ter no máximo 20 caracteres.");

        RuleForEach(x => x.Metadata.Features)
            .NotEmpty().WithMessage("Feature inválida.")
            .MaximumLength(50).WithMessage("A feature deve ter no máximo 50 caracteres.");

        RuleFor(x => x.Rating)
            .NotNull().WithMessage("A avaliação do jogo é obrigatória.");

        RuleFor(x => x.Rating.Average)
            .InclusiveBetween(0, 5).WithMessage("A média de avaliação deve estar entre 0 e 5.");

        RuleFor(x => x.Rating.Count)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade de avaliações não pode ser negativa.");
    }
}