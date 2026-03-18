using System.Text.Json.Serialization;
using FiapCloudGames.Catalog.Domain.Exceptions;

namespace FiapCloudGames.Catalog.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime ReleaseDate { get; private set; }
    public string Developer { get; private set; }
    public decimal Price { get; private set; }
    [JsonIgnore]

    private readonly List<Category> _categories = [];
    public IReadOnlyCollection<Category> Categories => _categories;

    private Game(){}
    public Game (string title, string description, DateTime releaseDate, string developer, decimal price, IEnumerable<Category> categories)
    {
        Id = Guid.NewGuid();
        Title = title;
        _categories.AddRange(categories);
        Description = description;
        ReleaseDate = releaseDate;
        Developer = developer;
        Price = price;

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

        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException("Descrição inválida. Deve ser informada.");
        if (Description.Length is < 10 or > 500)
            throw new DomainException("Descrição inválida. Deve conter entre 10 e 500 caracteres.");
        if (string.IsNullOrWhiteSpace(Developer))
            throw new DomainException("Desenvolvedor inválido. Deve ser informado.");
        if (Developer.Length > 80)
            throw new DomainException("Desenvolvedor inválido. Deve conter no máximo 80 caracteres.");
        if (Price < 0)
            throw new DomainException("Preço inválido. Não é permitido valores negativos.");
        
        if (ReleaseDate == default)
            throw new DomainException("Data de lançamento inválida.");

        if (ReleaseDate > DateTime.UtcNow)
            throw new DomainException("A data de lançamento não pode ser no futuro.");

        if (ReleaseDate.Year < 1950)
            throw new DomainException("Data de lançamento inválida.");
    }
}

