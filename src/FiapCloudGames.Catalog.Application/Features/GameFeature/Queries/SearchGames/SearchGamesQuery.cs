using FiapCloudGames.Catalog.Application.DTOs;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.GameFeature.Queries.SearchGames;

/// <summary>
/// Query de busca avançada de jogos via Elasticsearch (fuzzy + relevância),
/// independente da consulta tradicional do banco relacional.
/// </summary>
public sealed record SearchGamesQuery(string Term, int Page = 1, int Size = 20)
    : IRequest<IEnumerable<GetGameResponse>>;
