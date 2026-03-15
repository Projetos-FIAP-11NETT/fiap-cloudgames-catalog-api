using System.Text.Json.Serialization;
using FiapCloudGames.Catalog.Domain.Enums;
using FiapCloudGames.Catalog.Domain.Exceptions;

namespace FiapCloudGames.Catalog.Domain.Entities;

public class Order
{
    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    [JsonIgnore]
    public Game Game { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private Order() { }

    public Order(Guid userId, Game game)
    {
        UserId = userId;
        GameId = game.Id;
        Game = game;
        Status = OrderStatus.Pendente;
        CreatedAt = DateTime.UtcNow;
        TotalAmount = game.Price;

        Validate();
    }

    public void Validate()
    {
        if (UserId == Guid.Empty)
            throw new DomainException("UserId inválido. Deve ser informado.");

        if (GameId == Guid.Empty)
            throw new DomainException("GameId inválido. Deve ser informado.");

        if (TotalAmount < 0)
            throw new DomainException("O valor total do pedido não pode ser negativo.");
    }

    public void UpdateStatus(OrderStatus status)
    {
        if (Status == OrderStatus.Rejeitado)
            throw new DomainException("Não é possível alterar o status de um pedido cancelado");
        if(Status == OrderStatus.Aprovado)
            throw new DomainException("Não é possível alterar o status de um pedido já concluído");
        
        Status = status;
    }
}