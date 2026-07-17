namespace FiapCloudGames.Queue.Contracts;

/// <summary>
/// Evento publicado quando um jogo é criado ou editado no banco relacional,
/// sinalizando que o índice do Elasticsearch deve ser atualizado.
/// </summary>
public interface IGameUpserted
{
    Guid Id { get; }
    string Title { get; }
    string Description { get; }
    DateTime ReleaseDate { get; }
    string Developer { get; }
    decimal Price { get; }
    IEnumerable<Guid> Categories { get; }
}
