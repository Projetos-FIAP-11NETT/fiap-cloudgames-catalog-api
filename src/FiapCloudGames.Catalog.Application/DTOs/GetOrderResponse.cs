using FiapCloudGames.Catalog.Domain.Enums;

namespace FiapCloudGames.Catalog.Application.DTOs;

public class GetOrderResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}