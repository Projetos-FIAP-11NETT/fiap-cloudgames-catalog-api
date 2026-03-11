using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Domain.Entities;
using MediatR;

namespace FiapCloudGames.Catalog.Application.GameFeature.Queries.GetGame;

public sealed record class GetGameQuery
(
    Guid? Id,
    string? Title
)
    : IRequest<IEnumerable<GetGameReponse>>;
