using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data;
using FiapCloudGames.Catalog.Infrastructure.Repositories.Generic;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories;

public class OrderRepository(AppDbContext dataContext)
    : Repository<Order>(dataContext), IOrderRepository
{
}