namespace FiapCloudGames.Catalog.Domain.ReadModels;

public class LibraryItemReadModel
{
    public Guid UserId { get; init; }
    public List<GameInLibraryReadModel> Games { get; init; } = [];
}

public class GameInLibraryReadModel
{
    public Guid GameId { get; init; }
    public string GameTitle { get; init; } = string.Empty;
    public decimal GamePrice { get; init; }
    public int? OrderId { get; init; }
    public DateTime AddedAt { get; init; }
}