using FiapCloudGames.Catalog.Domain.Exceptions;

namespace FiapCloudGames.Catalog.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    
    public string Name { get; private set; }
    
    private readonly List<Game> _games = [];
    public IReadOnlyCollection<Game> Games => _games;
    
    public Category(string name)
    {
        Id = Guid.NewGuid();
        Name = name;

        Validate();
    }
    public Category() { }

    public Category(Guid id, string name)
    {
        Id = id;
        Name = name;

        Validate();
    }

    public Category(Guid id)
    {
        Id = id;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("Categoria inválida. Deve ser informado.");

        if (Name.Length < 2)
            throw new DomainException("Categoria inválida. Deve conter no mínimo 2 caracteres.");

        if (Name.Length > 40)
            throw new DomainException("Categoria inválida. Excedeu a quantidade máxima de 40 caracteres.");
    }
}