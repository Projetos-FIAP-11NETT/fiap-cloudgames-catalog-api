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
            .Include(o => o.Game)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
    {
        return await dataContext.Orders
            .Include(o => o.Game)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }
}