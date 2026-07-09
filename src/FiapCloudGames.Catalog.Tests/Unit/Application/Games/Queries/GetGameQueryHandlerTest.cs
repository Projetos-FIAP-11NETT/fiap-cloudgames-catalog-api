using FiapCloudGames.Catalog.Application.DTOs;
using FiapCloudGames.Catalog.Application.Features.GameFeature.Queries.GetGame;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Redis;
using FluentAssertions;
using Moq;
using System.Text.Json;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Games.Queries;

/// <summary>
/// Testes unitários do GetGameQueryHandler, responsável por buscar jogos
/// a partir do cache Redis, construindo a chave correta com base no filtro
/// e desserializando o resultado retornado.
/// </summary>
public class GetGameQueryHandlerTest
{
    private readonly Mock<IRedisRepository> _redisRepositoryMock = new();
    private readonly GetGameQueryHandler _handler;

    public GetGameQueryHandlerTest()
    {
        _handler = new GetGameQueryHandler(_redisRepositoryMock.Object);
    }

    /// <summary>
    /// Serializa uma lista de GetGameResponse para JSON, simulando o retorno do Redis.
    /// </summary>
    private static string SerializeGames(IEnumerable<GetGameResponse> games) =>
        JsonSerializer.Serialize(games);

    /// <summary>
    /// Cria uma lista de GetGameResponse com valores padrão.
    /// </summary>
    private static List<GetGameResponse> CreateGameResponses(int count = 2) =>
        Enumerable.Range(1, count).Select(i => new GetGameResponse
        {
            Id = Guid.NewGuid(),
            Title = $"Game {i}",
            Description = $"Descrição do Game {i}",
            ReleaseDate = new DateTime(2020, 1, i),
            Developer = "Developer Studio",
            Price = 59.90m * i,
            Categories = [Guid.NewGuid()]
        }).ToList();

    /// <summary>
    /// Garante que, quando o filtro é informado, a chave do Redis é construída
    /// com o filtro em letras minúsculas.
    /// </summary>
    [Fact]
    public async Task Handle_WhenFilterProvided_ShouldUseFilterLowercaseAsKey()
    {
        // Arrange
        var games = CreateGameResponses();
        var json = SerializeGames(games);

        _redisRepositoryMock
            .Setup(r => r.GetGameAsync("games:elden", "Elden", It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _handler.Handle(new GetGameQuery("Elden"), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        _redisRepositoryMock.Verify(r =>
            r.GetGameAsync("games:elden", "Elden", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que, quando o filtro não é informado (null), a chave padrão
    /// "games:" é utilizada.
    /// </summary>
    [Fact]
    public async Task Handle_WhenFilterIsNull_ShouldUseDefaultKey()
    {
        // Arrange
        var games = CreateGameResponses();
        var json = SerializeGames(games);

        _redisRepositoryMock
            .Setup(r => r.GetGameAsync("games:", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _handler.Handle(new GetGameQuery(null), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        _redisRepositoryMock.Verify(r =>
            r.GetGameAsync("games:", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que, quando o filtro é uma string vazia, a chave padrão
    /// "games:" é utilizada.
    /// </summary>
    [Fact]
    public async Task Handle_WhenFilterIsEmpty_ShouldUseDefaultKey()
    {
        // Arrange
        var games = CreateGameResponses();
        var json = SerializeGames(games);

        _redisRepositoryMock
            .Setup(r => r.GetGameAsync("games:", string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _handler.Handle(new GetGameQuery(string.Empty), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        _redisRepositoryMock.Verify(r =>
            r.GetGameAsync("games:", string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que, quando o filtro é um espaço em branco, a chave padrão
    /// "games:" é utilizada.
    /// </summary>
    [Fact]
    public async Task Handle_WhenFilterIsWhiteSpace_ShouldUseDefaultKey()
    {
        // Arrange
        var games = CreateGameResponses();
        var json = SerializeGames(games);

        _redisRepositoryMock
            .Setup(r => r.GetGameAsync("games:", "   ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _handler.Handle(new GetGameQuery("   "), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        _redisRepositoryMock.Verify(r =>
            r.GetGameAsync("games:", "   ", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que os campos do DTO são desserializados corretamente
    /// a partir do JSON retornado pelo Redis.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCacheReturnsData_ShouldDeserializeAllFieldsCorrectly()
    {
        // Arrange
        var expected = new GetGameResponse
        {
            Id = Guid.NewGuid(),
            Title = "Elden Ring",
            Description = "Action RPG",
            ReleaseDate = new DateTime(2022, 2, 25),
            Developer = "FromSoftware",
            Price = 199.90m,
            Categories = [Guid.NewGuid()]
        };

        var json = SerializeGames([expected]);

        _redisRepositoryMock
            .Setup(r => r.GetGameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _handler.Handle(new GetGameQuery("elden"), CancellationToken.None);

        // Assert
        var response = result.Single();

        response.Id.Should().Be(expected.Id);
        response.Title.Should().Be(expected.Title);
        response.Description.Should().Be(expected.Description);
        response.ReleaseDate.Should().Be(expected.ReleaseDate);
        response.Developer.Should().Be(expected.Developer);
        response.Price.Should().Be(expected.Price);
        response.Categories.Should().BeEquivalentTo(expected.Categories);
    }

    /// <summary>
    /// Garante que uma coleção vazia é retornada quando o Redis
    /// não encontra jogos para o filtro informado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCacheReturnsEmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var json = SerializeGames([]);

        _redisRepositoryMock
            .Setup(r => r.GetGameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _handler.Handle(new GetGameQuery("inexistente"), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Garante que o filtro é convertido para letras minúsculas independente
    /// do casing original informado na query.
    /// </summary>
    [Theory]
    [InlineData("ELDEN", "games:elden")]
    [InlineData("Elden Ring", "games:elden ring")]
    [InlineData("elden", "games:elden")]
    public async Task Handle_WhenFilterProvided_ShouldNormalizeKeyToLowercase(string filter, string expectedKey)
    {
        // Arrange
        var json = SerializeGames(CreateGameResponses(1));

        _redisRepositoryMock
            .Setup(r => r.GetGameAsync(expectedKey, filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        await _handler.Handle(new GetGameQuery(filter), CancellationToken.None);

        // Assert
        _redisRepositoryMock.Verify(r =>
            r.GetGameAsync(expectedKey, filter, It.IsAny<CancellationToken>()), Times.Once);
    }
}