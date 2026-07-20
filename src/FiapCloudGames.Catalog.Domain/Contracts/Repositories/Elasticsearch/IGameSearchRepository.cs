using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Repositories.Elasticsearch;

/// <summary>
/// Abstração do índice de busca avançada (Elasticsearch/OpenSearch).
/// Mantém o índice sincronizado com o banco relacional (indexação/remoção)
/// e expõe a busca com tolerância a erros (fuzzy) e ordenação por relevância,
/// de forma independente da consulta tradicional do Postgres.
/// </summary>
public interface IGameSearchRepository
{
    /// <summary>Indexa (cria ou atualiza) um jogo no índice de busca.</summary>
    Task IndexAsync(Game game, CancellationToken cancellationToken = default);

    /// <summary>Remove um jogo do índice de busca.</summary>
    Task DeleteAsync(Guid gameId, CancellationToken cancellationToken = default);

    /// <summary>Reindexa em lote uma coleção de jogos (backfill / reindex completo).</summary>
    Task BulkIndexAsync(IEnumerable<Game> games, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca jogos pelo termo informado com fuzzy search, retornando os
    /// resultados ordenados por relevância (score) e paginados.
    /// </summary>
    Task<IReadOnlyCollection<GameSearchResult>> SearchAsync(
        string term, int page, int size, CancellationToken cancellationToken = default);
}
