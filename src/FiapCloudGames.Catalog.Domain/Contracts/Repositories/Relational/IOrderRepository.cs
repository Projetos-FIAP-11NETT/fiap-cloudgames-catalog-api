using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational.Generic;
using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetOrdersByUserIdAsync(Guid userId);
    Task<int> AddOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(int orderId);
}