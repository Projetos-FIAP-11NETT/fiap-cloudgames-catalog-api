using FiapCloudGames.Catalog.Application.Features.CategoryFeature.Queries.GetCategory;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Categories.Queries;

/// <summary>
/// Testes unitários do GetCategoryQueryHandler, responsável por buscar categorias
/// filtrando por Id e/ou Nome e mapeando os resultados para GetCategoryResponse.
/// </summary>
public class GetCategoryQueryHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly GetCategoryQueryHandler _handler;

    public GetCategoryQueryHandlerTest()
    {
        _handler = new GetCategoryQueryHandler(_categoryRepositoryMock.Object);
    }

    /// <summary>
    /// Cria uma Category válida para uso nos testes.
    /// </summary>
    private static Category CreateCategory(string name = "RPG") => new(name);

    /// <summary>
    /// Garante que o Id e o Name da query são repassados corretamente ao repositório.
    /// </summary>
    [Fact]
    public async Task Handle_Always_ShouldCallRepositoryWithCorrectFilters()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string name = "RPG";
        var query = new GetCategoryQuery(id, name);

        _categoryRepositoryMock
            .Setup(r => r.GetCategory(id, name))
            .ReturnsAsync([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(r => r.GetCategory(id, name), Times.Once);
    }

    /// <summary>
    /// Garante que todos os campos do DTO são mapeados corretamente
    /// a partir da entidade Category.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCategoryExists_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var category = CreateCategory("Aventura");
        var query = new GetCategoryQuery(null, null);

        _categoryRepositoryMock
            .Setup(r => r.GetCategory(null, null))
            .ReturnsAsync([category]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var response = result.Single();

        response.Id.Should().Be(category.Id);
        response.Name.Should().Be(category.Name);
    }

    /// <summary>
    /// Garante que todas as categorias retornadas pelo repositório
    /// são mapeadas para GetCategoryResponse.
    /// </summary>
    [Fact]
    public async Task Handle_WhenMultipleCategoriesExist_ShouldReturnAllMappedResponses()
    {
        // Arrange
        var categories = new List<Category>
        {
            CreateCategory("RPG"),
            CreateCategory("Aventura"),
            CreateCategory("Estratégia")
        };

        var query = new GetCategoryQuery(null, null);

        _categoryRepositoryMock
            .Setup(r => r.GetCategory(null, null))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var responses = result.ToList();

        responses.Should().HaveCount(3);
        responses.Select(r => r.Name).Should().BeEquivalentTo(["RPG", "Aventura", "Estratégia"]);
    }

    /// <summary>
    /// Garante que uma coleção vazia é retornada quando o repositório
    /// não encontra categorias para os filtros informados.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoCategoriesFound_ShouldReturnEmptyCollection()
    {
        // Arrange
        var query = new GetCategoryQuery(Guid.NewGuid(), "Inexistente");

        _categoryRepositoryMock
            .Setup(r => r.GetCategory(query.Id, query.Name))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Garante que a busca funciona corretamente quando apenas o Id é informado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenOnlyIdProvided_ShouldCallRepositoryWithIdAndNullName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = CreateCategory("RPG");
        var query = new GetCategoryQuery(id, null);

        _categoryRepositoryMock
            .Setup(r => r.GetCategory(id, null))
            .ReturnsAsync([category]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);

        _categoryRepositoryMock.Verify(r => r.GetCategory(id, null), Times.Once);
    }

    /// <summary>
    /// Garante que a busca funciona corretamente quando apenas o Name é informado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenOnlyNameProvided_ShouldCallRepositoryWithNullIdAndName()
    {
        // Arrange
        const string name = "Esportes";
        var category = CreateCategory(name);
        var query = new GetCategoryQuery(null, name);

        _categoryRepositoryMock
            .Setup(r => r.GetCategory(null, name))
            .ReturnsAsync([category]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);

        _categoryRepositoryMock.Verify(r => r.GetCategory(null, name), Times.Once);
    }

    /// <summary>
    /// Garante que a busca funciona corretamente quando nenhum filtro é informado,
    /// retornando todas as categorias.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoFilterProvided_ShouldCallRepositoryWithBothNulls()
    {
        // Arrange
        var categories = new List<Category>
        {
            CreateCategory("RPG"),
            CreateCategory("Ação")
        };

        var query = new GetCategoryQuery(null, null);

        _categoryRepositoryMock
            .Setup(r => r.GetCategory(null, null))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        _categoryRepositoryMock.Verify(r => r.GetCategory(null, null), Times.Once);
    }
}
