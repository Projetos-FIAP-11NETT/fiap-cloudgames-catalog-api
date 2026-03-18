using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.CategoryFeature.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler
    (
        ICategoryRepository categoryRepository
    )
    : IRequestHandler<UpdateCategoryCommand, bool>
{

    public async Task<bool> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var existingCategory = await categoryRepository.GetByIdAsync(command.Id);

        if (existingCategory == null)
            throw new NotFoundException("Categoria não encontrada.");

        var updatedCategory = new Domain.Entities.Category(command.Id, command.Name);

        categoryRepository.Update(updatedCategory);

        return await categoryRepository.SaveChangesAsync(cancellationToken);
    }
}