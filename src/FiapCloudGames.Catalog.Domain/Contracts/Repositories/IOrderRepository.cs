using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Generic;
using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetOrdersByUserIdAsync(string userId);
}