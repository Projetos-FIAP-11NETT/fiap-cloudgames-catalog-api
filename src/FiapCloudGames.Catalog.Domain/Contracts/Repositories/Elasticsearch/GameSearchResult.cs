namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.Elasticsearch;

/// <summary>
/// Representa um resultado da busca avançada (Elasticsearch), já projetado
/// a partir do documento indexado. A ordem da coleção retornada reflete a
/// relevância (score) calculada pelo motor de busca.
/// </summary>
public sealed record GameSearchResult(
    Guid Id,
    string Title,
    string Description,
    DateTime ReleaseDate,
    string Developer,
    decimal Price,
    IReadOnlyCollection<Guid> Categories);
