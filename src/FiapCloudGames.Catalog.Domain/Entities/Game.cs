using FiapCloudGames.Catalog.Domain.Exceptions;
using FiapCloudGames.Catalog.Domain.MarkingInterfaces;

namespace FiapCloudGames.Catalog.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }

    private readonly List<Category> _categories = [];
    public IReadOnlyCollection<Category> Categories => _categories;

    private Game(){}
    public Game (string title, IEnumerable<Category> categories)
    {
        Id = Guid.NewGuid();
        Title = title;
        _categories.AddRange(categories);

        Validate();
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new DomainException("Título inválido. Deve ser informado.");

        if (Title.Length > 80)
            throw new DomainException("Título inválido. Excedeu a quantidade máxima de 80 caracteres.");

        if (_categories.Count == 0)
            throw new DomainException("O jogo deve ter pelo menos uma categoria.");

        if (_categories
            .GroupBy(c => c.Id)
            .Any(g => g.Count() > 1))
        {
            throw new DomainException("Categoria duplicada não é permitida.");
        }
    }
}

