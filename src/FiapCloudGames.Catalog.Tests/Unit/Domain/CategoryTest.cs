using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;

namespace FiapCloudGames.Catalog.Tests.Unit.Domain;

public class CategoryTest
{
    #region Name validations

    [Fact]
    public void MustFailWhenNameIsEmpty()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Category("")
        );

        Assert.Equal("Categoria inválida. Deve ser informado.", exception.Message);
    }

    [Fact]
    public void MustFailWhenNameIsTooShort()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Category("a")
        );

        Assert.Equal("Categoria inválida. Deve conter no mínimo 2 caracteres.", exception.Message);
    }

    [Fact]
    public void MustFailWhenNameIsTooLong()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Category(new string('a', 41))
        );

        Assert.Equal("Categoria inválida. Excedeu a quantidade máxima de 40 caracteres.", exception.Message);
    }

    #endregion Name validations

    [Theory]
    [InlineData("RPG")]
    [InlineData("2D")]
    [InlineData("Esporte")]
    public void MustBeCreatedWhenTheCategoryIsValid(string name)
    {
        var category = new Category(name);

        Assert.Equal(category.Name, name);
    }
}