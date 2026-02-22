using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;

namespace FiapCloudGames.Catalog.Tests.Unit.Domain;

public class GameTest
{
    
    #region Title validations

    [Fact]
    public void MustFailWhenTitleIsEmpty()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Game("", [])
        );

        Assert.Equal("Título inválido. Deve ser informado.", exception.Message);
    }

    [Fact]
    public void MustFailWhenTitleIsTooLong()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Game(new string('a', 81), [])
        );

        Assert.Equal("Título inválido. Excedeu a quantidade máxima de 80 caracteres.", exception.Message);
    }

    #endregion Title validations

    #region Category validations

    [Fact]
    public void MustFailWhenCategoryIsEmpty()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Game(new string('a', 50), [])
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
            new Game(new string('a', 50), categories)
        );

        Assert.Equal("Categoria duplicada não é permitida.", exception.Message);
    }

    #endregion Category validations

    [Theory]
    [InlineData("Age of Empires", "Ação")]
    [InlineData("Fifa 2026", "RPG")]
    [InlineData("CS", "Estratégia")]
    public void MustBeCreatedWhenTheGameIsValid(string title, string categoryName)
    {
        var category = new Category(categoryName);

        var game = new Game(title, [category]);

        Assert.Equal(game.Title, title);
    }
}