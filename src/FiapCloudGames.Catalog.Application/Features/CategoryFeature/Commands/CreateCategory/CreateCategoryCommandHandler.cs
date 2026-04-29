using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Entities;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.CategoryFeature.Commands.CreateCategory;

public class CreateCategoryCommandHandler
(
    ICategoryRepository categoryRepository
)
    : IRequestHandler<CreateCategoryCommand, bool>
{

    public async Task<bool> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var exists = await categoryRepository.AnyCategoryByNameAsync(command.Name);

        if (exists)
            throw new BusinessException($"Já existe a categoria inserida.");
        
        var categoryResult = new Category(command.Name);
        await categoryRepository.AddAsync(categoryResult);
        return await categoryRepository.SaveChangesAsync(cancellationToken);
    }
}