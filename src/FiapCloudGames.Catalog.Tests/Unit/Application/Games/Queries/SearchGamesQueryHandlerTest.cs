using FiapCloudGames.Catalog.Application.Features.GameFeature.Queries.SearchGames;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Elasticsearch;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Games.Queries;

/// <summary>
/// Testes unitários do SearchGamesQueryHandler, responsável pela busca avançada
/// de jogos via Elasticsearch (independente do banco relacional), validando o
/// mapeamento dos resultados para GetGameResponse e a preservação da ordem de
/// relevância retornada pelo motor de busca.
/// </summary>
public class SearchGamesQueryHandlerTest
{
    private readonly Mock<IGameSearchRepository> _searchRepositoryMock = new();
    private readonly SearchGamesQueryHandler _handler;

    public SearchGamesQueryHandlerTest()
    {
        _handler = new SearchGamesQueryHandler(_searchRepositoryMock.Object);
    }

    /// <summary>
    /// Cria um GameSearchResult com valores padrão para os testes.
    /// </summary>
    private static GameSearchResult BuildResult(
        string title = "The Witcher 3",
        string developer = "CD Projekt Red",
        Guid? id = null,
        IReadOnlyCollection<Guid>? categories = null) =>
        new(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: "Um RPG de mundo aberto.",
            ReleaseDate: new DateTime(2015, 5, 19),
            Developer: developer,
            Price: 99.90m,
            Categories: categories ?? [Guid.NewGuid()]);

    /// <summary>
    /// Garante que os resultados retornados pelo Elasticsearch são mapeados
    /// corretamente para GetGameResponse, preservando todos os campos.
    /// </summary>
    [Fact]
    public async Task Handle_WhenResultsFound_ShouldMapAllFieldsToResponse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var result = BuildResult(categories: [categoryId]);
        var query = new SearchGamesQuery("witcher");

        _searchRepositoryMock
            .Setup(r => r.SearchAsync(query.Term, query.Page, query.Size, It.IsAny<CancellationToken>()))
            .ReturnsAsync([result]);

        // Act
        var response = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        response.Should().HaveCount(1);
        var mapped = response[0];
        mapped.Id.Should().Be(result.Id);
        mapped.Title.Should().Be(result.Title);
        mapped.Description.Should().Be(result.Description);
        mapped.ReleaseDate.Should().Be(result.ReleaseDate);
        mapped.Developer.Should().Be(result.Developer);
        mapped.Price.Should().Be(result.Price);
        mapped.Categories.Should().ContainSingle().Which.Should().Be(categoryId);
    }

    /// <summary>
    /// Garante que a ordem de relevância (score) devolvida pelo Elasticsearch
    /// é preservada na resposta, sem reordenação no handler.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPreserveRelevanceOrder()
    {
        // Arrange
        var first = BuildResult(title: "Most Relevant");
        var second = BuildResult(title: "Less Relevant");
        var third = BuildResult(title: "Least Relevant");
        var query = new SearchGamesQuery("relevant");

        _searchRepositoryMock
            .Setup(r => r.SearchAsync(query.Term, query.Page, query.Size, It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second, third]);

        // Act
        var response = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        response.Select(x => x.Title)
            .Should()
            .ContainInOrder("Most Relevant", "Less Relevant", "Least Relevant");
    }

    /// <summary>
    /// Garante que o termo, página e tamanho informados na query são repassados
    /// corretamente ao repositório de busca.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldForwardTermPageAndSizeToRepository()
    {
        // Arrange
        var query = new SearchGamesQuery("elden ring", Page: 2, Size: 5);

        _searchRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _searchRepositoryMock.Verify(
            r => r.SearchAsync("elden ring", 2, 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Garante que, quando nenhum resultado é encontrado, uma coleção vazia
    /// é retornada (e não nulo).
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoResults_ShouldReturnEmptyCollection()
    {
        // Arrange
        var query = new SearchGamesQuery("inexistente");

        _searchRepositoryMock
            .Setup(r => r.SearchAsync(query.Term, query.Page, query.Size, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEmpty();
    }
}
