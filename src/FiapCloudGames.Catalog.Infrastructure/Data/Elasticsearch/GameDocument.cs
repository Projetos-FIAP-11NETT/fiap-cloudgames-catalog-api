namespace FiapCloudGames.Catalog.Infrastructure.Data.Elasticsearch;

public class GameDocument
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Developer { get; set; }
    public decimal Price { get; set; }
    public List<Guid> Categories { get; set; } = [];
}
