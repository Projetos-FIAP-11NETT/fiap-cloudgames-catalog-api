using FiapCloudGames.Catalog.Domain.Contracts.Repositories;
using FiapCloudGames.Catalog.Domain.Exceptions;
using MediatR;

namespace FiapCloudGames.Catalog.Application.CategoryFeature.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler
    (
        ICategoryRepository userRepository
    )
    : IRequestHandler<UpdateCategoryCommand, bool>
{

    public async Task<bool> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var existingCategory = await userRepository.GetByIdAsync(command.Id);

        if (existingCategory == null)
            throw new NotFoundException("Categoria não encontrada.");

        existingCategory.GetType()
            .GetProperty("Name")?
            .SetValue(existingCategory, command.Name);

        userRepository.Update(existingCategory);

        return await userRepository.SaveChangesAsync(cancellationToken);
    }


}