using FiapCloudGames.Catalog.Application.Features.CategoryFeature.Commands.DeleteCategory;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Categories.Commands;

/// <summary>
/// Testes unitários do DeleteCategoryCommandHandler, responsável por remover uma categoria
/// validando sua existência e persistindo a remoção.
/// </summary>
public class DeleteCategoryCommandHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTest()
    {
        _handler = new DeleteCategoryCommandHandler(_categoryRepositoryMock.Object);
    }

    /// <summary>
    /// Cria uma Category válida para uso nos testes.
    /// </summary>
    private static Category CreateCategory(string name = "RPG") => new(name);

    /// <summary>
    /// Garante que, quando a categoria existe e SaveChanges é bem-sucedido,
    /// a categoria é removida e true é retornado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldRemoveCategoryAndReturnTrue()
    {
        // Arrange
        var category = CreateCategory();
        var command = new DeleteCategoryCommand(category.Id);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(category);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _categoryRepositoryMock.Verify(r => r.Remove(category), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que uma NotFoundException é lançada quando a categoria
    /// informada no comando não existe no repositório.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync((Category?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Categoria*");

        _categoryRepositoryMock.Verify(r => r.Remove(It.IsAny<Category>()), Times.Never);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que false é retornado quando SaveChanges falha após a remoção da categoria.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFalse()
    {
        // Arrange
        var category = CreateCategory();
        var command = new DeleteCategoryCommand(category.Id);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(category);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        _categoryRepositoryMock.Verify(r => r.Remove(category), Times.Once);
    }

    /// <summary>
    /// Garante que o Id informado no comando é repassado corretamente ao repositório
    /// para busca da categoria a ser removida.
    /// </summary>
    [Fact]
    public async Task Handle_Always_ShouldQueryRepositoryWithCorrectCategoryId()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(categoryId), Times.Once);
    }
}
