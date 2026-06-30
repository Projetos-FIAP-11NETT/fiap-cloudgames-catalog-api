using FiapCloudGames.Catalog.Application.Features.CategoryFeature.Commands.CreateCategory;
using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Postgres;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace FiapCloudGames.Catalog.Tests.Unit.Application.Categories.Commands;

/// <summary>
/// Testes unitários do CreateCategoryCommandHandler, responsável por criar categorias
/// validando duplicidade pelo nome e persistindo o resultado.
/// </summary>
public class CreateCategoryCommandHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTest()
    {
        _handler = new CreateCategoryCommandHandler(_categoryRepositoryMock.Object);
    }

    /// <summary>
    /// Garante que, quando o nome da categoria é único, a categoria é persistida
    /// e true é retornado.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldCreateCategoryAndReturnTrue()
    {
        // Arrange
        var command = new CreateCategoryCommand("RPG");

        _categoryRepositoryMock
            .Setup(r => r.AnyCategoryByNameAsync(command.Name))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Garante que uma BusinessException é lançada quando já existe uma categoria
    /// com o mesmo nome cadastrada.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCategoryAlreadyExists_ShouldThrowBusinessException()
    {
        // Arrange
        var command = new CreateCategoryCommand("RPG");

        _categoryRepositoryMock
            .Setup(r => r.AnyCategoryByNameAsync(command.Name))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*categoria*");

        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Garante que false é retornado quando SaveChanges falha após a criação da categoria.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFalse()
    {
        // Arrange
        var command = new CreateCategoryCommand("Aventura");

        _categoryRepositoryMock
            .Setup(r => r.AnyCategoryByNameAsync(command.Name))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Garante que o nome informado no comando é repassado corretamente
    /// à verificação de duplicidade no repositório.
    /// </summary>
    [Fact]
    public async Task Handle_Always_ShouldCheckDuplicityWithCorrectName()
    {
        // Arrange
        var command = new CreateCategoryCommand("Estratégia");

        _categoryRepositoryMock
            .Setup(r => r.AnyCategoryByNameAsync(command.Name))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();

        _categoryRepositoryMock.Verify(r => r.AnyCategoryByNameAsync("Estratégia"), Times.Once);
    }

    /// <summary>
    /// Garante que a categoria adicionada ao repositório é criada com
    /// o nome informado no comando.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValid_ShouldAddCategoryWithCorrectName()
    {
        // Arrange
        var command = new CreateCategoryCommand("Esportes");

        _categoryRepositoryMock
            .Setup(r => r.AnyCategoryByNameAsync(command.Name))
            .ReturnsAsync(false);
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(r =>
            r.AddAsync(It.Is<Category>(c => c.Name == "Esportes")), Times.Once);
    }
}
