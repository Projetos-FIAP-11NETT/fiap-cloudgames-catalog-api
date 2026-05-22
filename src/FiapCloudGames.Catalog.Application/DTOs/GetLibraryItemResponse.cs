namespace FiapCloudGames.Catalog.Application.DTOs;

public class GetLibraryItemResponse
{
    public Guid GameId { get; set; }
    public string GameTitle { get; set; }
    public int? OrderId { get; set; }
    public DateTime AddedAt { get; set; }
}