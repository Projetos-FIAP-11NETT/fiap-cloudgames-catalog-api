using FiapCloudGames.Catalog.Domain.Enums;
using MediatR;

namespace FiapCloudGames.Catalog.Application.OrderFeature.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand (int OrderId, OrderStatus Status) : IRequest<bool>;