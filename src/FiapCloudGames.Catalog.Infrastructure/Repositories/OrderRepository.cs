using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories;

public class OrderRepository(AppDbContext dataContext)
    : Repository<Order>(dataContext), IOrderRepository
{
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await dataContext.Orders
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
    {
        return await dataContext.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task<int> AddOrderAsync(Order order)
    {
        var newOrder =await dataContext.Orders.AddAsync(order);
        await dataContext.SaveChangesAsync();
        return newOrder.Entity.Id;
    }
}