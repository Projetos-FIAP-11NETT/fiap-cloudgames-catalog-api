using FiapCloudGames.Catalog.Application.DTOs;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.LibraryItemFeature.Queries.GetLibraryItemsByUser;

public sealed record class GetLibraryItemsByUserQuery(Guid UserId) : IRequest<IEnumerable<GetLibraryItemResponse>>;