using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data.Relational;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Postgres.Generic;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.Postgres;

public class OrderRepository(AppDbContext dataContext)
    : Repository<Order>(dataContext), IOrderRepository
{
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await dataContext.Orders
            .AsNoTracking()
            .Include(x=>x.Game)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
    {
        return await dataContext.Orders
            .AsNoTracking()
            .Include(x=>x.Game)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task<int> AddOrderAsync(Order order)
    {
        var newOrder =await dataContext.Orders.AddAsync(order);
        await dataContext.SaveChangesAsync();
        return newOrder.Entity.Id;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await dataContext.Orders.AsNoTracking().Include(x=>x.Game).FirstOrDefaultAsync(x=> x.Id == orderId);
    }
}