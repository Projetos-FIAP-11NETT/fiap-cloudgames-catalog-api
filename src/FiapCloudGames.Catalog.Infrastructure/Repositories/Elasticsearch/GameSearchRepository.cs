using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Elasticsearch;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Infrastructure.Data.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FiapCloudGames.Catalog.Infrastructure.Repositories.Elasticsearch;

public class GameSearchRepository(
    ElasticsearchClient client,
    IOptions<ElasticsearchSettings> settings,
    ILogger<GameSearchRepository> logger
) : IGameSearchRepository
{
    private readonly string _indexName = settings.Value.IndexName;

    public async Task IndexAsync(Game game, CancellationToken cancellationToken = default)
    {
        var document = ToDocument(game);

        var response = await client.IndexAsync(
            document,
            idx => idx.Index(_indexName).Id(document.Id.ToString()),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            logger.LogError("[catalog-service] Falha ao indexar o jogo {GameId} no Elasticsearch: {Error}",
                game.Id, response.DebugInformation);
        }
    }

    public async Task DeleteAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync<GameDocument>(
            gameId.ToString(),
            d => d.Index(_indexName),
            cancellationToken);

        if (!response.IsValidResponse && response.Result != Elastic.Clients.Elasticsearch.Result.NotFound)
        {
            logger.LogError("[catalog-service] Falha ao remover o jogo {GameId} do Elasticsearch: {Error}",
                gameId, response.DebugInformation);
        }
    }

    public async Task BulkIndexAsync(IEnumerable<Game> games, CancellationToken cancellationToken = default)
    {
        var documents = games.Select(ToDocument).ToList();
        if (documents.Count == 0)
            return;

        var response = await client.BulkAsync(b => b
            .Index(_indexName)
            .IndexMany(documents, (descriptor, document) => descriptor.Id(document.Id.ToString())),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            logger.LogError("[catalog-service] Falha ao reindexar jogos em lote no Elasticsearch: {Error}",
                response.DebugInformation);
        }
    }

    public async Task<IReadOnlyCollection<GameSearchResult>> SearchAsync(
        string term, int page, int size, CancellationToken cancellationToken = default)
    {
        var from = Math.Max(0, (page - 1) * size);

        var response = await client.SearchAsync<GameDocument>(s => s
            .Indices(_indexName)
            .From(from)
            .Size(size)
            .Query(q => q
                .MultiMatch(m => m
                    .Query(term)
                    .Fields(new[] { "title", "description", "developer" })
                    .Fuzziness(new Fuzziness("AUTO")))),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            logger.LogError("[catalog-service] Falha ao buscar jogos no Elasticsearch: {Error}",
                response.DebugInformation);
            return [];
        }

        return response.Documents
            .Select(d => new GameSearchResult(
                d.Id, d.Title, d.Description, d.ReleaseDate, d.Developer, d.Price, d.Categories))
            .ToList();
    }

    private static GameDocument ToDocument(Game game) => new()
    {
        Id = game.Id,
        Title = game.Title,
        Description = game.Description,
        ReleaseDate = game.ReleaseDate,
        Developer = game.Developer,
        Price = game.Price,
        Categories = [.. game.Categories.Select(c => c.Id)]
    };
}
