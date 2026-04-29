using FiapCloudGames.Catalog.Domain.Contracts.Repositories.Relational;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.Features.CategoryFeature.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler
    (
        ICategoryRepository categoryRepository
    )
    : IRequestHandler<DeleteCategoryCommand, bool>
{

    public async Task<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var existingCategory = await categoryRepository.GetByIdAsync(command.Id);

        if (existingCategory == null)
            throw new NotFoundException("Categoria não encontrada.");

        categoryRepository.Remove(existingCategory);

        return await categoryRepository.SaveChangesAsync(cancellationToken);
    }

}