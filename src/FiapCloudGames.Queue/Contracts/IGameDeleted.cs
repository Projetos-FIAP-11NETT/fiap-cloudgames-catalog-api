namespace FiapCloudGames.Queue.Contracts;

/// <summary>
/// Evento publicado quando um jogo é removido do banco relacional,
/// sinalizando que o documento correspondente deve ser excluído do índice.
/// </summary>
public interface IGameDeleted
{
    Guid GameId { get; }
}
