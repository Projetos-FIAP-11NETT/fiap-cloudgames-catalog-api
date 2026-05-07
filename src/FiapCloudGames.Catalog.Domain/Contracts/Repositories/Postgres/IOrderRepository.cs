using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres.Generic;
using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetOrdersByUserIdAsync(Guid userId);
    Task<int> AddOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(int orderId);
}