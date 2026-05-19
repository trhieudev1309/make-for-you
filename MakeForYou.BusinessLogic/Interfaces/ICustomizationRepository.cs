using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface ICustomizationRepository
    {
        // Get all customization groups for a product (including options)
        Task<List<CustomizationGroup>> GetCustomizationGroupsByProductIdAsync(long productId);

        // Get a specific customization group with its options
        Task<CustomizationGroup?> GetCustomizationGroupByIdAsync(long customizationGroupId);

        // Add a new customization group
        Task<CustomizationGroup> AddCustomizationGroupAsync(CustomizationGroup customizationGroup);

        // Update an existing customization group
        Task<CustomizationGroup> UpdateCustomizationGroupAsync(CustomizationGroup customizationGroup);

        // Delete a customization group (cascade deletes options)
        Task DeleteCustomizationGroupAsync(long customizationGroupId);

        // Add a new customization option
        Task<CustomizationOption> AddCustomizationOptionAsync(CustomizationOption customizationOption);

        // Update an existing customization option
        Task<CustomizationOption> UpdateCustomizationOptionAsync(CustomizationOption customizationOption);

        // Delete a customization option
        Task DeleteCustomizationOptionAsync(long customizationOptionId);

        // Get all options for a specific customization group
        Task<List<CustomizationOption>> GetCustomizationOptionsByGroupIdAsync(long customizationGroupId);

        // Delete all customization groups for a product (when deleting product)
        Task DeleteCustomizationGroupsByProductIdAsync(long productId);
    }
}