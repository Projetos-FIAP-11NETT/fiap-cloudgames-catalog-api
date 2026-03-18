using System.Text.Json.Serialization;
using FiapCloudGames.Catalog.Domain.Exceptions;

namespace FiapCloudGames.Catalog.Domain.Entities;

public class LibraryItem
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    [JsonIgnore]
    public Game Game { get; private set; }
    public int? OrderId { get; private set; }
    [JsonIgnore]
    public Order? Order { get; private set; }
    public DateTime AddedAt { get; private set; }

    private LibraryItem() { }

    public LibraryItem(Guid userId, Game game, int? orderId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        GameId = game.Id;
        Game = game;
        OrderId = orderId;
        AddedAt = DateTime.UtcNow;

        Validate();
    }

    public void Validate()
    {
        if (UserId == Guid.Empty)
            throw new DomainException("UserId inválido. Deve ser informado.");

        if (GameId == Guid.Empty)
            throw new DomainException("GameId inválido. Deve ser informado.");
    }
}