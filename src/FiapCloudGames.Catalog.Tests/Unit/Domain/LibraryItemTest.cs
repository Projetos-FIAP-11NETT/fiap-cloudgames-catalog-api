using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;

namespace FiapCloudGames.Catalog.Tests.Unit.Domain;

public class LibraryItemTest
{
    private readonly Guid ValidUserId = Guid.NewGuid();

    private static Game ValidGame =>
        new("Game Test", "Descrição válida", new DateTime(2020, 1, 1), "Dev Studio", 49.90m,
            new List<Category> { new("RPG") });

    #region UserId validations

    [Fact]
    public void MustFailWhenUserIdIsEmpty()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new LibraryItem(Guid.Empty, ValidGame)
        );

        Assert.Equal("UserId inválido. Deve ser informado.", exception.Message);
    }

    #endregion

    #region Valid LibraryItem

    [Fact]
    public void MustBeCreatedWhenLibraryItemIsValidWithoutOrder()
    {
        var game = ValidGame;
        var libraryItem = new LibraryItem(ValidUserId, game);

        Assert.Equal(ValidUserId, libraryItem.UserId);
        Assert.Equal(game.Id, libraryItem.GameId);
        Assert.Null(libraryItem.OrderId);
        Assert.NotEqual(Guid.Empty, libraryItem.Id);
        Assert.NotEqual(default, libraryItem.AddedAt);
    }

    [Fact]
    public void MustBeCreatedWhenLibraryItemIsValidWithOrder()
    {
        var game = ValidGame;
        var orderId = 42;
        var libraryItem = new LibraryItem(ValidUserId, game, orderId);

        Assert.Equal(ValidUserId, libraryItem.UserId);
        Assert.Equal(game.Id, libraryItem.GameId);
        Assert.Equal(orderId, libraryItem.OrderId);
        Assert.NotEqual(Guid.Empty, libraryItem.Id);
        Assert.NotEqual(default, libraryItem.AddedAt);
    }

    [Fact]
    public void MustHaveNullOrderIdWhenCreatedWithoutOrder()
    {
        var libraryItem = new LibraryItem(ValidUserId, ValidGame);

        Assert.Null(libraryItem.OrderId);
    }

    #endregion
}