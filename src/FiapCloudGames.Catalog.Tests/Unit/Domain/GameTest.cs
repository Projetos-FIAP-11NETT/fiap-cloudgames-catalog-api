using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;

namespace FiapCloudGames.Catalog.Tests.Unit.Domain;

public class GameTest
{
    private static readonly DateTime ValidReleaseDate = new(2020, 1, 1);
    private const string ValidDescription = "Descrição válida";
    private const string ValidDeveloper = "Dev Studio";
    private const decimal ValidPrice = 50;

    private static List<Category> ValidCategories =>
        new() { new Category("RPG") };

    #region Title validations

    [Fact]
    public void MustFailWhenTitleIsEmpty()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Game("", ValidDescription, ValidReleaseDate, ValidDeveloper, ValidPrice, ValidCategories)
        );

        Assert.Equal("Título inválido. Deve ser informado.", exception.Message);
    }

    [Fact]
    public void MustFailWhenTitleIsTooLong()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Game(new string('a', 81), ValidDescription, ValidReleaseDate, ValidDeveloper, ValidPrice, ValidCategories)
        );

        Assert.Equal("Título inválido. Excedeu a quantidade máxima de 80 caracteres.", exception.Message);
    }

    #endregion

    #region Category validations

    [Fact]
    public void MustFailWhenCategoryIsEmpty()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game Test", ValidDescription, ValidReleaseDate, ValidDeveloper, ValidPrice, [])
        );

        Assert.Equal("O jogo deve ter pelo menos uma categoria.", exception.Message);
    }

    [Fact]
    public void MustFailWhenDuplicateCategories()
    {
        var category = new Category("RPG");

        var categories = new List<Category>
        {
            category,
            category
        };

        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game Test", ValidDescription, ValidReleaseDate, ValidDeveloper, ValidPrice, categories)
        );

        Assert.Equal("Categoria duplicada não é permitida.", exception.Message);
    }

    #endregion

    #region Description validations
    [Fact]
    public void MustFailWhenDescriptionIsEmpty()
    {
        var category = new Category("RPG");
        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game test","", ValidReleaseDate, ValidDeveloper, ValidPrice, [category])
        );

        Assert.Equal("Descrição inválida. Deve ser informada.", exception.Message);
    }
    [Fact]
    public void MustFailWhenDescriptionIsTooLong()
    {
        var category = new Category("RPG");
        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game test",new string('a', 501), ValidReleaseDate, ValidDeveloper, ValidPrice, [category])
        );

        Assert.Equal("Descrição inválida. Deve conter entre 10 e 500 caracteres.", exception.Message);
    }
    [Fact]
    public void MustFailWhenDescriptionIsTooShort()
    {
        var category = new Category("RPG");
        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game test","teste", ValidReleaseDate, ValidDeveloper, ValidPrice, [category])
        );

        Assert.Equal("Descrição inválida. Deve conter entre 10 e 500 caracteres.", exception.Message);
    }
    
    #endregion

    #region Developer validations
    [Fact]
    public void MustFailWhenDeveloperIsNull()
    {
        var category = new Category("RPG");
        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game test",ValidDescription, ValidReleaseDate,"",ValidPrice, [category])
        );
        Assert.Equal("Desenvolvedor inválido. Deve ser informado.", exception.Message);
    }
    
    [Fact]
    public void MustFailWhenDeveloperIsTooLong()
    {
        var category = new Category("RPG");
        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game test",ValidDescription, ValidReleaseDate,new string('a', 81),ValidPrice, [category])
        );
        Assert.Equal("Desenvolvedor inválido. Deve conter no máximo 80 caracteres.", exception.Message);
    }

    #endregion
    
    #region Price validations

    [Fact]
    public void MustFailWhenPriceIsNegative()
    {
        var category = new Category("RPG");
        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game test",ValidDescription, ValidReleaseDate, ValidDeveloper,-5, [category])
        );
        Assert.Equal("Preço inválido. Não é permitido valores negativos.", exception.Message);
    }
    
    #endregion
    
    #region ReleaseDate validations
    
    [Fact]
    public void MustFailWhenReleaseDateIsDefault()
    {
        var category = new Category("RPG");
        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game test",ValidDescription, new DateTime(), ValidDeveloper,ValidPrice, [category])
        );
        Assert.Equal("Data de lançamento inválida.", exception.Message);
    }
    [Fact]
    public void MustFailWhenReleaseDateIsFuture()
    {
        var category = new Category("RPG");
        var exception = Assert.Throws<DomainException>(() =>
            new Game("Game test",ValidDescription, DateTime.UtcNow.AddDays(10),ValidDeveloper,ValidPrice, [category])
        );
        Assert.Equal("A data de lançamento não pode ser no futuro.", exception.Message);
    }
    #endregion
    
    
    [Theory]
    [InlineData("Age of Empires", "Jogo de estratégia vista de cima", "2005-10-15", "Microsoft", 49.90, "Estratégia")]
    [InlineData("Fifa 2026", "Jogo de futebol", "2025-09-20", "EA Sports", 299.90, "Esporte")]
    [InlineData("CS", "Counter Strike, jogo de tiro 5x5", "2000-11-09", "Valve", 19.90, "FPS")]
    public void MustBeCreatedWhenTheGameIsValid(
        string title,
        string description,
        string releaseDate,
        string developer,
        decimal price,
        string categoryName)
    {
        var category = new Category(categoryName);

        var game = new Game(
            title,
            description,
            DateTime.Parse(releaseDate),
            developer,
            price,
            new List<Category> { category });

        Assert.Equal(title, game.Title);
        Assert.Equal(description, game.Description);
        Assert.Equal(developer, game.Developer);
        Assert.Equal(price, game.Price);
        Assert.Single(game.Categories);
    }
}