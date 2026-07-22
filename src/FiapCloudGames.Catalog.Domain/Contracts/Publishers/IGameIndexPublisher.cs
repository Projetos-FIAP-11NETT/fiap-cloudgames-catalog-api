using FiapCloudGames.Catalog.Domain.Entities;

namespace FiapCloudGames.Catalog.Domain.Contracts.Publishers;

/// <summary>
/// Publica eventos de indexação para manter o índice de busca (Elasticsearch)
/// sincronizado com o banco relacional, de forma assíncrona e desacoplada.
/// </summary>
public interface IGameIndexPublisher
{
    /// <summary>Publica o evento de criação/edição de um jogo para (re)indexação.</summary>
    Task PublishUpsertedAsync(Game game, CancellationToken cancellationToken = default);

    /// <summary>Publica o evento de exclusão de um jogo para remoção do índice.</summary>
    Task PublishDeletedAsync(Guid gameId, CancellationToken cancellationToken = default);
}
