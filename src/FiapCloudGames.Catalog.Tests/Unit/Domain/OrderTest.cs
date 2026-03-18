using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Enums;
using FiapCloudGames.Catalog.Domain.Exceptions;

namespace FiapCloudGames.Catalog.Tests.Unit.Domain;

public class OrderTest
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
            new Order(Guid.Empty, ValidGame)
        );

        Assert.Equal("UserId inválido. Deve ser informado.", exception.Message);
    }

    #endregion

    #region Valid Order

    [Fact]
    public void MustBeCreatedWhenOrderIsValid()
    {
        var game = ValidGame;
        var order = new Order(ValidUserId, game);

        Assert.Equal(ValidUserId, order.UserId);
        Assert.Equal(game.Id, order.GameId);
        Assert.Equal(game.Price, order.TotalAmount);
        Assert.Equal(OrderStatus.Pendente, order.Status);
        Assert.Null(order.PaidAt);
        Assert.NotEqual(0, order.Id);
    }

    #endregion
}